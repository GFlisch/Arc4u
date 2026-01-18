# Custom Root CA.

On a cluster, to handle HTTP/2 and TLS 1.2/1.3 we need to create certificates for each services.
The certificates issued by the cluster is managed by cert-manager.

When deploying a service in a cluster, via cert-manager we can connect to it using https or gRPCs.
But the certificate authority is not registered on each node running the service.

One option is during the docker build to add the root CA to the container. This is fine and make sense.
If you don't want to touch the image and be able to deploy the service on any cluster, you can use the Kubernetes secret to store the root CA certificate and mount it as a volume in the container.
And you will have to configure your http client to trust the certificate.

this is exactly what we do here.
