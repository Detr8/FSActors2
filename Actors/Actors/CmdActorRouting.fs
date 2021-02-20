module CmdActorRouting

open Akka.FSharp
open Microsoft.Extensions.Logging
open Messages

let private CmdActoryBody (logger:ILogger) state (mailbox:Actor<ExchangeMessage>)=
   let rec imp state=actor{
       let! message=mailbox.Receive()
       logger.LogInformation("A new cmdmessage in cmdActor {message}", message)

       //logic here

       //Update state in superviser
       mailbox.Context.Parent <! state

       return! imp state
   }

   imp state

let GetCmdActorHandler (logger:ILogger) (msg: ExchangeMessage) state (mailbox:Actor<ExchangeMessage>)=
    let cmd=MessageUtils.ParseMsg msg.Name

    match cmd with
    |FastCalcIC 
    |FastCalcResult -> CmdHandlers.CalcFastHandler logger state mailbox
    |_ ->CmdActoryBody logger state mailbox