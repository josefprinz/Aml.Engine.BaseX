declare variable $file external;
let $fileNode:=doc($file)
 return <XElements>
  {$fileNode/CAEXFile/ExternalReference}
 </XElements>