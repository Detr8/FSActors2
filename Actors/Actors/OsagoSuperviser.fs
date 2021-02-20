module OsagoSuperviser
open System
open Microsoft.Extensions.Logging
open Akka.FSharp
open Messages
open Akka.Actor

let findOrCreateChildActor (logger:ILogger) name handler (mailbox: Actor<ExchangeMessage>)=
    let existingActor=mailbox.Context.Child(name)
    if existingActor.IsNobody() then 
        logger.LogInformation("{name} actor has been created", name)
        (handler |> spawn mailbox name)
    else 
        existingActor

let cmdActorHandler (logger:ILogger) state (mailbox:Actor<ExchangeMessage>)=
    let rec loop state=actor{
        let! message = mailbox.Receive ()
        logger.LogInformation("A new cmdmessage in cmdActor {message}", message)
        let enrichMsg={message with Name="enrich"}
        mailbox.Context.Parent <! enrichMsg

        return! loop state
    }
    loop state

let getCmdActor (logger:ILogger) (state:OsagoStateItem) message (mailbox: Actor<ExchangeMessage>) =  
    //findOrCreateChildActor message.Name (cmdActorHandler logger state) mailbox
    findOrCreateChildActor logger message.Name (CmdActorRouting.GetCmdActorHandler logger message state) mailbox

let cardProcessorHandler (logger:ILogger) initialState (mailbox: Actor<ExchangeMessage>) = 
    
    let (|Inner|External|) (cmdName:string)=
        let cmd=MessageUtils.ParseMsg cmdName
        match cmd with
        |Other ->
            match cmdName.ToLower() with
            |"enrichstate"->Inner
            |_->Inner
        |_->External

        
    
    let rec loop state = actor {
        let! message = mailbox.Receive ()      
        logger.LogInformation("A new cmdmessage in cardProcessor {message}", message)

        (*
            parse cmd
            find actor
            each actor can handle cmd, cmdResult and other
        *)
        match message.Name with
        |Inner -> 
            //check enrichstate
            if message.Name.ToLower()="enrichstate" then 
                logger.LogInformation("enrich state for card {message.TraceId}", message.TraceId)
                return! loop {state with LastUpdateDate=DateTime.Now} //enrich body
            
            ()
        |External->
            let cmdActor=getCmdActor logger state message mailbox
            cmdActor <! message

        return! loop state
    }
    loop initialState 

let getCardProcessorActor  (logger:ILogger) (mailbox:Actor<ExchangeMessage>) traceId=   
    let initialState=
        {
            CreationDate=DateTime.Now;
            LastUpdateDate=DateTime.Now;
            TraceId=traceId;
            DataJson=None
        }    
    findOrCreateChildActor logger traceId (cardProcessorHandler logger initialState) mailbox

let OsagoSuperviserActor (logger:ILogger) (mailbox:Actor<ExchangeMessage>)=
    let rec imp ()=
        actor{
            let! msg=mailbox.Receive()
            logger.LogInformation("A new cmdmessage in superviser {message}", msg)
            let cardProcessor=getCardProcessorActor logger mailbox msg.TraceId

            cardProcessor <! msg

            return! imp()
        }
        
    imp()

