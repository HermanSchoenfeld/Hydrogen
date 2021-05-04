﻿namespace Sphere10.Framework.Protocol {
	public interface IProtocolHub<TEndpoint, TMessageID, TMessageType, TNonce, TPayload, TMessage, TCommand, TRequest, TResponse, THandshake, TProtocolChannel>
		where THandshake : IProtocolHandshake<TEndpoint, TMessageID, TMessageType, TNonce, TPayload>
		where TMessage : IProtocolMessage<TEndpoint, TMessageID, TMessageType, TNonce, TPayload>
		where TCommand : IProtocolCommand<TEndpoint, TMessageID, TMessageType, TNonce, TPayload>
		where TRequest : IProtocolRequest<TEndpoint, TMessageID, TMessageType, TNonce, TPayload>
		where TResponse : IProtocolResponse<TEndpoint, TMessageID, TMessageType, TNonce, TPayload>
		where TProtocolChannel : IProtocolChannel<TEndpoint, TMessageID, TMessageType, TNonce, TPayload, TMessage, TCommand, TRequest, TResponse> {

		int MaxChannels { get; init; }

		TProtocolChannel ReceiveConnection(THandshake handshake);

		TProtocolChannel InitiateConnection(THandshake handshake);
	}


}