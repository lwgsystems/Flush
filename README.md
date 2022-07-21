# ScrumPokerClub (formerly Flush) is now deprecated in favour of [ScrumKit](https://github.com/LeifWritesCode/ScrumKit), a more powerful application spanning more of the Agile lifecycle. The existing 'latest' build (e581ce737d1f7e0fccfcb9c9bd5632a8e746dc84) and 'edge' build (v1.0.0) may continue to be used at [spc.leif.uk](https://spc.leif.uk) and [edge.spc.leif.uk](https://edge.spc.leif.uk) respectively. This repository will not longer receive code updates, with the exception of security fixes.

![Flush][logo]

## What is this?

ScrumPokerClub is a tool for facilitating and enhancing Agile planning activities; built with speed, ease of use and reliability in mind.

## Prerequisites

- Docker 17.06+
- Docker Compose 3.0+

## Getting Started

All of the following steps assume you have checked out the application and `PWD` is the root of such.

### Pull Submodules

Please issue `git submodule init && git submodule update` to ensure the latest versions of the submodules are present at build time.

### Run ScrumPokerClub

To build the container image and start `scrumpokerclub.app`, run `docker-compose up` at the root of your checkout.

## Enabling HTTPS

By default ScrumPokerClub assumes it is run behind a mature reverse proxy and, as a result, HTTPS is not enabled. To enable HTTPS, please add the following to the `Kestrel.EndPoints` section of `appsettings.json`.

```
"HttpsInlineCertFile": {
  "Url": "https://<host>:<port>",
  "Certificate": {
    "Path": "/app/tls/cert.pfx",
    "Password": "<password for your cert.pfx>"
  }
}
```

In the above section, replace the variables with values that suit your environment. If your certificate does not have a password, set `Password` to a blank string. The certificate should be stored at the root of your checkout, in a folder named `certs`.

If you are running a development copy of this application, you can use the default dotnet development certificates by substituting the above with the below snippet. Again, please replace the variables with values that suit your environment.

```
"HttpsDefaultCert": {
  "Url": "https://<host>:<port>"
}
```

## Changelog

See [this document](Docs/CHANGELOG.md).

## Known Issues

See [this document](Docs/KNOWNISSUES.md).

[logo]: Docs/flush-logo.png
