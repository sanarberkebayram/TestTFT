# Project Initialization (Empty Project)

1) Create folder structure under `Assets/_Project/Scripts`.
2) Implement EventBus (`IGlobalEventBus`, `ISceneEventBus`) and concrete buses.
3) Add `ProjectInstaller` and `SceneInstaller` and ensure contexts reference installers.
4) Initialize minimal shared utilities.
5) Sequence: Architect → Implementation → Integration → QA.
6) No gameplay before infrastructure exists.
