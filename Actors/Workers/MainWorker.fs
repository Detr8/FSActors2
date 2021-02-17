namespace Workers

open RabbitMQ.Client
open RabbitMQ.Client.Events
open System.Text
open Akka.FSharp
open Akka.Actor

module MainWorker=
   open Microsoft.Extensions.Logging
   open Microsoft.Extensions.Configuration
   open Microsoft.Extensions.Hosting
   open System
   open System.Threading.Tasks 
   open FsUtils
   open Messages

   type MainWorker(logger: ILogger<MainWorker>, config: IConfiguration, system: ActorSystem)=
   //system: ActorSystem,
   //autoModels: AutoModels.AutoModelsClient) =
        inherit BackgroundService()
        let _logger = logger

        let host =
            config.GetValue<string>("RabbitMQ:Hostname")

        let username =
            config.GetValue<string>("RabbitMQ:Username")

        let password =
            config.GetValue<string>("RabbitMQ:Password")

        let factory =
            new ConnectionFactory(HostName = host, Password = password, UserName = username)

        let connection = factory.CreateConnection()
        let channel = connection.CreateModel()

        let listenQueue="OsagoActors"

        override bs.ExecuteAsync stoppingToken =
            printfn "MainWorker is working..."
            channel.QueueDeclare(listenQueue, false, false, false, null) |> ignore
            printfn $"Listening %s{listenQueue} stream"
            
            let actor=(OsagoSuperviser.OsagoSuperviserActor logger) |> spawn system "super"

            let f: Async<unit> =
                async {
                    let consumer = new EventingBasicConsumer(channel)
                    consumer.Received.Add(fun args ->
                        let jsonStr = Encoding.UTF8.GetString(args.Body.ToArray())
                        use logScope=logger.BeginScope("{action}", "OsagoProcessing")
                        logger.LogInformation($"A new command %s{jsonStr}")

                        let msg=JsonUtils.Deserialize<ExchangeMessage> jsonStr

                        actor <! msg
                        (*
                            main actor has state with cardProcessors by traceId
                            when a new message has been recieved -> 
                                find superviser by traceId, if not - create 
                                find cmdActor or create it
                                cmdActor <! msg should mutate state

                            actors:
                            1. router - stores state
                            2. processor
                            3. cmdProcessors
                        *)
                    )

                    channel.BasicConsume(listenQueue, true, consumer) |> ignore

                }
            Async.StartAsTask f :> Task