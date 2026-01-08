$fileName = "309"
$baseFolder = 'C:\temp\'
$folder = $baseFolder + $fileName
 
if (Test-Path $baseFolder) {
       Write-Host "Base Path exsits"
} else {
        New-Item -Path $baseFolder -ItemType Directory
       
}
 
if (Test-Path $folder) {
        Remove-Item $folder -verbose -Recurse -Force
} else {
        Write-Host "EXport path doesn't exsits, will create"
}
 
New-Item -Path $folder -ItemType Directory
 
C:\\ffmpeg\\bin\\ffmpeg.exe -i C:\temp\input-$fileName.mp4 -ss 00:00:00 -frames:v 1 C:\temp\$fileName\$fileName.jpg
 
C:\\ffmpeg\\bin\\ffmpeg.exe -i C:\temp\input-$fileName.mp4 -vcodec h264 -b:a 96k -start_number 0 -hls_time 10 -hls_list_size 0 -f hls -hls_enc 1 C:\temp\$fileName\$fileName.m3u8


