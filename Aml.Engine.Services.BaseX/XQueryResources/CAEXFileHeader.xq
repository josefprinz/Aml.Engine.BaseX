declare variable $file external;
 let $fileNode:=doc($file)/CAEXFile
 return <CAEXFile>
     {$fileNode/@*} 
     {$fileNode/Description}
     {$fileNode/Version}
     {$fileNode/Revision}
     {$fileNode/Copyright}
     {$fileNode/SourceObjectInformation}
     {$fileNode/AdditionalInformation}
     {$fileNode/SuperiorStandardVersion}
     {$fileNode/SourceDocumentInformation}  
     {$fileNode/ExternalReference}
 </CAEXFile>