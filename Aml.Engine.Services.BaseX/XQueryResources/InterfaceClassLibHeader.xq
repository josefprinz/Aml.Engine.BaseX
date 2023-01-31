declare variable $file external;
let $fileNode:=doc($file)
 return <XElements>
 {
	for $objectNode in $fileNode/CAEXFile/InterfaceClassLib
	 return <InterfaceClassLib>
     {$objectNode/@*} 
     {$objectNode/Description}
     {$objectNode/Version}
     {$objectNode/Revision}
     {$objectNode/Copyright}
     {$objectNode/SourceObjectInformation}
     {$objectNode/AdditionalInformation}
	</InterfaceClassLib>
 }
 </XElements>