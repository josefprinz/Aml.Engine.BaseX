declare variable $file external;
let $fileNode:=doc($file)
 return <XElements>
 {
	for $objectNode in $fileNode/CAEXFile/RoleClassLib
	 return <RoleClassLib>
     {$objectNode/@*} 
     {$objectNode/Description}
     {$objectNode/Version}
     {$objectNode/Revision}
     {$objectNode/Copyright}
     {$objectNode/SourceObjectInformation}
     {$objectNode/AdditionalInformation}
	</RoleClassLib>
 }
 </XElements>