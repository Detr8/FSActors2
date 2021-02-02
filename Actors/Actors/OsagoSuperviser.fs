﻿


module OsagoSuperviser
open System
open Microsoft.Extensions.Logging
open Akka.FSharp
open Messages
open Akka.Actor


let OsagoSuperviserHandler (logger:ILogger) (state:OsagoStateItem list) (msg:ExchangeMessage)=
    //parse messageName
    //find processor or create it

    let existingStateItem= state |> List.tryFind(fun x->x.TraceId=msg.TraceId)
    match existingStateItem with
    |None ->
        let newStateItem= {
            CreationDate=DateTime.Now;
            LastUpdateDate=DateTime.Now;
            TraceId=msg.TraceId;
            DataJson=msg.BodyJson
        }
        newStateItem :: state
    |_->state


//let OsagoCardProcessorHandler (logger:ILogger) (state:OsagoStateItem) (msg:ExchangeMessage)=
    

let OsagoSuperviserActor (cardProcessorHandler) (logger:ILogger) (mailbox:Actor<ExchangeMessage>)=
    
    let getCardProcessorActor (mailbox:Actor<ExchangeMessage>) traceId=
        let actorRef= mailbox.Context.Child(traceId)

        if actorRef.IsNobody() then (cardProcessorHandler |> spawn mailbox traceId)
        else actorRef

    let rec imp (state:OsagoStateItem list)=
        actor{
            let! msg=mailbox.Receive()            
            let cardProcessor=getCardProcessorActor mailbox msg.TraceId

            let newState=OsagoSuperviserHandler logger state msg //mutate state

            cardProcessor <! msg

            return! imp newState
        }
        
    imp []
