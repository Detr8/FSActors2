module Messages

open System
open Akka.Actor


type Commands=
    |CreateAndCalc
    |CreateAndIssue
    |FastCalcResult
    |FullCalcResult
    |IssueResult
    |CommissionComputedResult


type ExchangeMessage={
    Name:string;
    BodyJson:string option;
    TraceId:string;
}

type OsagoStateItem={
    CreationDate: DateTime;
    LastUpdateDate:DateTime;
    TraceId:string;
    DataJson:string option;
}