Issue description: 
Result of 'File.Exists(...)' is cached for few seconds 
(i.e., File.Exists(...) for one of containers returns false while for another containers it returns true)

Application reproduces file system inconsistency issue when multiple containerized application instances perform create and check for existence of a single file

How to use:
1. Create a directory provided in 'docker-compose.yaml -> services -> volumes -> source' (or specify here and create other directory)
2. Build the application
3. Run docker-compose up --build --scale app=5
4. See Console output - after file is created some of containers can still return 'False' for 'File.Exists(...)' for some time
5. Remove the directory created in step 1
