# CentralStore

A small distributed app consisting of 2 api(services) CentralStore and LocalStore. Both of them are used for simple CRUD on Products.
Central store api has the ability when a Product is created/deleted/updated to specify which Store Id(Identifies the Local store) the operation is outomatically also run on.
LocalStore doesn't have the ability above. When a CRUD operation is done on it, it outomatically sends the info required to do the operation on CentralStore side too.

Prerequesites for running the app:
1. Docker Desktop.

There are 2 ways to run the app. Using Visual Studio:
1. Open the .sln from Visual Studio and select Docker Compose as the Run Profile.
2. Run it.
3. Once the containers are up and running go to [CentralStoreLocalUrl](http://localhost:5002/swagger) - http://localhost:5002/swagger/index.html. This will open the swagger page for CentralStore.
4. Go to [LocalStoreLocalUrl](http://localhost:5003/swagger) - http://localhost:5003/swagger/index.html for the swagger page for LocalStore.

Using CLI of choice:
1. In a cli tool(powershell, bash, etc.) go to the project source folder where the docker-compose file is located.
2. Run docker compose up -d from that folder.
3. Once all the container are up and running use the URLs shown in the above sections 3. and 4.

In the project source folder there is a folder by the name of SampleRequests and inside there is a file called SampleReuqestForSwagger.txt. Inside there will be sample request bodies to use with swagger.
