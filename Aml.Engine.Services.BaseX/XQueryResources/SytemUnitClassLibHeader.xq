declare variable $file external;
let $fileNode:=doc($file)
 return <XElements>
 {
	for $objectNode in $fileNode/CAEXFile/SystemUnitClassLib
	 return <SystemUnitClassLib>
     {$objectNode/@*} 
     {$objectNode/Description}
     {$objectNode/Version}
     {$objectNode/Revision}
     {$objectNode/Copyright}
     {$objectNode/SourceObjectInformation}
     {$objectNode/AdditionalInformation}
	</SystemUnitClassLib>
 }
 </XElements>