module Messages

open System
open Akka.Actor


type Commands=
    |SaveCard
    |FastCalc
    |FullCalc
    |PolicyIssue
    |RefreshPayUrl
    |GetFiles

type CommandResults=
    |ICFastCalcResult
    |ICFullCalcResult
    |PolicyIssueResult
    |SaveCardResult

type HighLevelCommands=
    |CreateAndCalc
    |CreateAndIssue

type Message=
    |HighLevelCommand of HighLevelCommands
    |CommandResult of CommandResults
    |OsagoCommand of Commands


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