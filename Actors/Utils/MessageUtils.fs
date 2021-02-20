module MessageUtils

open Messages

let ParseMsg (msg:string)=
    let parse=function 
    |"createcard" -> CreateCard
    |"fastcalcic"->FastCalcIC
    |"fastcalcicres"->FastCalcResult
    |"fullcalcic"->FullCalcIC
    |"fullcalcicres"->FullCalcResult
    |_->Other
    
    msg.ToLower() |> parse