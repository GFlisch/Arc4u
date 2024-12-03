using Grpc.Core;
using Grpc.Core.Interceptors;

namespace Arc4u.gRPC.Interceptors;

/// <summary>
/// The relative path interceptor sufixes the service name.
/// When used with a proxy like Yarp and a sufix is created to route the service to a specifi service
/// the interceptor can be used.
/// </summary>
public abstract class AddSuffixPathInterceptor : Interceptor
{
    protected AddSuffixPathInterceptor(string relativePathUrl)
    {
        _relativePathUrl = relativePathUrl.Trim();
    }

    readonly string _relativePathUrl;
    public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(TRequest request, ClientInterceptorContext<TRequest, TResponse> context, AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
    {
        CreateContext(ref context);

        return continuation(request, context);
    }

    public override TResponse BlockingUnaryCall<TRequest, TResponse>(TRequest request, ClientInterceptorContext<TRequest, TResponse> context, BlockingUnaryCallContinuation<TRequest, TResponse> continuation)
    {
        CreateContext(ref context);

        return continuation(request, context);
    }

    public override AsyncClientStreamingCall<TRequest, TResponse> AsyncClientStreamingCall<TRequest, TResponse>(ClientInterceptorContext<TRequest, TResponse> context, AsyncClientStreamingCallContinuation<TRequest, TResponse> continuation)
    {
        CreateContext(ref context);

        return continuation(context);
    }

    public override AsyncServerStreamingCall<TResponse> AsyncServerStreamingCall<TRequest, TResponse>(TRequest request, ClientInterceptorContext<TRequest, TResponse> context, AsyncServerStreamingCallContinuation<TRequest, TResponse> continuation)
    {
        CreateContext(ref context);

        return continuation(request, context);
    }

    public override AsyncDuplexStreamingCall<TRequest, TResponse> AsyncDuplexStreamingCall<TRequest, TResponse>(ClientInterceptorContext<TRequest, TResponse> context, AsyncDuplexStreamingCallContinuation<TRequest, TResponse> continuation)
    {
        CreateContext(ref context);

        return continuation(context);
    }

    private Method<TRequest, TResponse> GetMethod<TRequest, TResponse>(Method<TRequest, TResponse> method) => new Method<TRequest, TResponse>(method.Type, $"{_relativePathUrl}{method.ServiceName}", method.Name, method.RequestMarshaller, method.ResponseMarshaller);

    private void CreateContext<TRequest, TResponse>(ref ClientInterceptorContext<TRequest, TResponse> context)
                 where TRequest : class
                 where TResponse : class
    {
        context = new ClientInterceptorContext<TRequest, TResponse>(GetMethod(context.Method), context.Host, context.Options);
    }

}
