Issue description: 
Result of 'File.Exists(...)' is cached for few seconds (i.e., File.Exists(...) can return 'false' when file actually exists and can be listed by Directory.GetFiles(...))

Application reproduces file system inconsistency issue when multiple containerized application instances perform create/update/delete operations on single file

How to use:
1. Create a directory provided in 'docker-compose.yaml -> services -> volumes -> source' (or specify here and create other directory)
2. Build the application
3. Run docker-compose up --build --scale app=10
4. Wait till Console output contains "The issue is reproduced: Contradiction between File.Exists (false) and Directory.GetFiles (contains the file)" message
   and "File was found after {nnnn} milliseconds" message
   First message means that for some reasons File.Exists returns false while it can be listed by Directory.GetFiles
   Second message means that after some wait File.Exist returns true
5. After issue is reproduced containers can be stopped by Ctrl+C, docker-compose down
6. Observe content of the files created by applications - it is expected that each container created 10 xml items with indexes from 1 to 10.
   When issue is reproduced some items are missing (starting index for some container is not '1') - it means that file was overwritten after it was created because
   some container have got 'FileNotFound' exception on attempt to open file that was created by another container 
   (See 'Containers_file_content_after_issue_is_reproduced.png' for reference)
7. Remove files created by application

Note: if Program.cs -> line 27 (CheckDocumentExistence(documentName);) is removed, the issue will not be reproduced.
It means that when we do not execute File.Exists(...) before file is created (so we do not cache 'false' result) 
we do not get 'FileNotFound' exception on attempt to open file that was created by another container