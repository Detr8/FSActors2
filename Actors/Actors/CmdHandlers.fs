module CmdHandlers

open Akka.FSharp
open Microsoft.Extensions.Logging
open Messages

let CalcFastHandler (logger:ILogger) state (mailbox:Actor<ExchangeMessage>)=
    let rec imp state=actor{
        let! message=mailbox.Receive()
        logger.LogInformation("A new cmdmessage in cmdActor {message}", message)

        //logic here
        let cmd=MessageUtils.ParseMsg message.Name
        match cmd with
        |FastCalcIC ->()
        |FastCalcResult ->()

        //Update state in superviser
        mailbox.Context.Parent <! state

        return! imp state
    }

    imp state

let CalcFulltHandler (logger:ILogger) state (mailbox:Actor<ExchangeMessage>)=
    let rec imp state=actor{
        let! message=mailbox.Receive()
        logger.LogInformation("A new cmdmessage in cmdActor {message}", message)

        //logic here

        //Update state in superviser
        mailbox.Context.Parent <! state

        return! imp state
    }

    imp state