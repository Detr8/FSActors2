module CmdHandlers

open Akka.FSharp
open Microsoft.Extensions.Logging
open Messages
open System

let CalcFastHandler (logger:ILogger) (state:OsagoStateItem) (mailbox:Actor<ExchangeMessage>)=
    let rec imp (state:OsagoStateItem)=actor{
        let! message=mailbox.Receive()
        

        //logic here
        let cmd=MessageUtils.ParseMsg message.Name
        match cmd with
        |FastCalcIC ->
            //send via rmq
            //Async.Sleep(TimeSpan.FromSeconds(30.0)) 
            logger.LogInformation("A new FastCalc req in cmdActor {message}", message)

        |FastCalcResult ->
            logger.LogInformation("A new FastCalcResp req in cmdActor {message}", message)
            mailbox.Context.Parent <! {message with Name="enrichstate"}

        //Update state in superviser
        

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