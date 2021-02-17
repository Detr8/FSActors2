namespace FsUtils

open FSharp.Json

module JsonUtils=
    let Serialize data=
        let jsonConfig = JsonConfig.create(jsonFieldNaming = Json.lowerCamelCase)
        let res = Json.serializeEx jsonConfig data
        res


    let Deserialize<'T> jsonStr=
        let jsonConfig = JsonConfig.create(jsonFieldNaming = Json.lowerCamelCase, allowUntyped = true)
        let res= Json.deserializeEx<'T> jsonConfig jsonStr
        res

