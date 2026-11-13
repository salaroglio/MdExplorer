import { HubConnection } from '@microsoft/signalr';

/**
 * I tempi di tutte le connessioni SignalR del client, uguali a quelli del servizio (Startup: KeepAliveInterval 15 s,
 * ClientTimeoutInterval 2 min). 13/11/2026, dal cliente: una risposta lunga di MarkAgent tagliata da «connessione persa»:
 * con 30 s di pazienza bastava un servizio sotto carico o una finestra in secondo piano per chiudere la connessione, e
 * con lei la sessione dell'agente. Il client manda un segnale ogni 15 s e aspetta quelli del servizio fino a 2 minuti.
 */
export function applySignalrTimeouts(connection: HubConnection): HubConnection {
  connection.keepAliveIntervalInMilliseconds = 15_000;
  connection.serverTimeoutInMilliseconds = 120_000;
  return connection;
}
