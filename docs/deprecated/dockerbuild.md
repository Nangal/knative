```
As of Knative 0.8, Knative Build has been deprecated in favor of Tekton Pipelines. This doc is kept as a reference for pre-0.8 Knative installations. Please refer to Tekton Pipelines section of the tutorial on how to do builds in Knative going forward.
```

# Docker Hub Build

In the [previous lab](helloworldbuild.md), we built and pushed a container image to Google Cloud Registry (GCR). In this lab, we will push to Docker Hub instead. It's more involved as we need to register secrets for Docker Hub.

There's [Orchestrating a source-to-URL deployment on Kubernetes](https://www.knative.dev/docs/serving/samples/source-to-url-go/) tutorial in Knative docs that explains how to do this and more but we will go through the steps here as well.

## Register secrets for Docker Hub

We need to first register a secret in Kubernetes for authentication with Docker Hub.

Create a [docker-secret.yaml](../build/deprecated/docker-secret.yaml) file for `Secret` manifest, which is used to store your Docker Hub credentials:

```yaml
apiVersion: v1
kind: Secret
metadata:
  name: basic-user-pass
  annotations:
    build.knative.dev/docker-0: https://index.docker.io/v1/
type: kubernetes.io/basic-auth
data:
  # Use 'echo -n "username" | base64' to generate this string
  username: BASE64_ENCODED_USERNAME
  # Use 'echo -n "password" | base64' to generate this string
  password: BASE64_ENCODED_PASSWORD
```

Make sure to replace `BASE64_ENCODED_USERNAME` and `BASE64_ENCODED_PASSWORD` with your Base64 encoded DockerHub username and password.

Create a [service-account.yaml](../build/deprecated/service-account.yaml) for `Service Account` used to link the build process to the secret:

```yaml
apiVersion: v1
kind: ServiceAccount
metadata:
  name: build-bot
secrets:
  - name: basic-user-pass
```

Apply the `Secret` and `Service Account`:

```bash
kubectl apply -f docker-secret.yaml
secret "basic-user-pass" created
kubectl apply -f service-account.yaml
serviceaccount "build-bot" created
```

## Design the build

We will use [Kaniko](https://github.com/GoogleContainerTools/kaniko) again in our Build. Create a [build-helloworld-docker.yaml](../build/deprecated/build-helloworld-docker.yaml) build file:

```yaml
apiVersion: build.knative.dev/v1alpha1
kind: Build
metadata:
  name: build-helloworld-docker
spec:
  serviceAccountName: build-bot
  source:
    git:
      url: https://github.com/nikhilbarthwal/knative.git
      revision: master
    subPath: serving/helloworld/csharp
  steps:
  - name: build-and-push
    image: "gcr.io/kaniko-project/executor:v0.6.0"
    args:
    - "--dockerfile=/workspace/Dockerfile"
    # Replace {username} with your actual DockerHub
    - "--destination=docker.io/{username}/helloworld:build"
```

This uses Knative Build to download the source code in the 'workspace' directory and then use Kaniko to build and push an image to Docker Hub tagged with `knativebuild`. Note how we're using `build-bot` as `serviceAccountName`.

## Run and watch the build

You can start the build with:

```bash
kubectl apply -f build-helloworld-docker.yaml
```

Check that it is created:

```bash
kubectl get build
```

Soon after, you'll see a pod created for the build:

```bash
kubectl get pods
NAME                                             READY     STATUS
build-helloworld-docker-pod-454bd8        0/1       Init:2/3
```

You can see the progress of the build with:

```bash
kubectl logs --follow --container=build-step-build-and-push <podid>
```

When the build is finished, you'll see the pod in `Completed` state:

```bash
kubectl get pods
NAME                                              READY     STATUS
build-helloworld-docker-pod-454bd8         0/1       Completed
```

At this point, you should see the image pushed to Docker Hub:

![Docker Hub](../images/dockerhub.png)

## What's Next?

[Kaniko Build Template](kanikobuildtemplate.md)
