module MessageUtils

open Messages

let ParseMsg (msg:string)=
    let parse=function 
    |"createcard" -> CreateCard
    |"fastcalcic"->FastCalcIC
    |"fullcalcic"->FullCalcIC
    |_->Other
    
    msg.ToLower() |> parse