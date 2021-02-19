module OsagoSuperviser
open System
open Microsoft.Extensions.Logging
open Akka.FSharp
open Messages
open Akka.Actor

let findOrCreateChildActor name handler (mailbox: Actor<ExchangeMessage>)=
    let existingActor=mailbox.Context.Child(name)
    if existingActor.IsNobody() then (handler |> spawn mailbox name)
    else existingActor

let cmdActorHandler (logger:ILogger) state (mailbox:Actor<ExchangeMessage>)=
    let rec loop state=actor{
        let! message = mailbox.Receive ()
        logger.LogInformation("A new cmdmessage in cmdActor {message}", message)
        let enrichMsg={message with Name="enrich"}
        mailbox.Context.Parent <! enrichMsg

        return! loop state
    }
    loop state

let getCmdActor (logger:ILogger) state message (mailbox: Actor<ExchangeMessage>) =  
    //findOrCreateChildActor message.Name (cmdActorHandler logger state) mailbox
    findOrCreateChildActor message.Name (CmdActorRouting.GetCmdActorHandler logger message state) mailbox

let cardProcessorHandler (logger:ILogger) initialState (mailbox: Actor<ExchangeMessage>) = 
    
    let (|Inner|Result|Request|) (cmdName:string)=
        match cmdName with
        |"saveAndCalc"|"saveAndIssue"->Request
        |c when c.EndsWith("Result") ->Result
        |_->Inner
    
    let rec loop state = actor {
        let! message = mailbox.Receive ()      
        logger.LogInformation("A new cmdmessage in cardProcessor {message}", message)

        (*
            parse cmd
            find actor
            each actor can handle cmd, cmdResult and other
        *)

        if message.Name="enrich" then 
            printfn $"enrich state for %s{message.TraceId}"
            return! loop {state with LastUpdateDate=DateTime.Now} //enrich body
        
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
    findOrCreateChildActor traceId (cardProcessorHandler logger initialState) mailbox

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

