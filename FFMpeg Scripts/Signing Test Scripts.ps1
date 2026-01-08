
$FileNameSourceLesson = "C:\Temp\SOL Signing Video Test\lesson-2-23.mp4"
$FileNameSourceSigning = "C:\Temp\SOL Signing Video Test\signing-2-23.mp4"
$FileNameSourceSigningPortrait = "C:\Temp\SOL Signing Video Test\signing-portrait-2-23.mp4"

$FileNameTopRight = "C:\Temp\SOL Signing Video Test\Output\output-topright.mp4"
$FileNameBottomRight = "C:\Temp\SOL Signing Video Test\Output\output-bottomright.mp4"
$FileNameSplitEqual = "C:\Temp\SOL Signing Video Test\Output\output-splitequal.mp4"
$FileNameSplitMainLesson = "C:\Temp\SOL Signing Video Test\Output\output-splitmainlesson.mp4"
$FileNameBottomRightEmbed = "C:\Temp\SOL Signing Video Test\Output\output-bottomrightembed.mp4"
$FileNameBottomRightEmbedOffset = "C:\Temp\SOL Signing Video Test\Output\output-bottomrightembedoffset.mp4"

$FileNameBottomRightEmbedOffsetEncypted = "C:\Temp\SOL Signing Video Test\Output\output-bottomrightembedoffset.mu38"

if (Test-Path $FileNameTopRight) {
   Remove-Item $FileNameTopRight -verbose
}

if (Test-Path $FileNameBottomRight) {
   Remove-Item $FileNameBottomRight -verbose
}

if (Test-Path $FileNameSplitEqual) {
   Remove-Item $FileNameSplitEqual -verbose
}

if (Test-Path $FileNameSplitMainLesson) {
   Remove-Item $FileNameSplitMainLesson -verbose
}

if (Test-Path $FileNameBottomRightEmbed) {
   Remove-Item $FileNameBottomRightEmbed -verbose
}

if (Test-Path $FileNameBottomRightEmbedOffset) {
   Remove-Item $FileNameBottomRightEmbedOffset -verbose
}

if (Test-Path $FileNameBottomRightEmbedOffsetEncypted) {
   Remove-Item $FileNameBottomRightEmbedOffsetEncypted -verbose
}


#Shrink osurce files
#C:\ffmpeg\bin\ffmpeg.exe -i "C:\Temp\SOL Signing Video Test\signing-portrait-2-23.mp4" -c:v libx264 -c:a copy  -t 30 "C:\Temp\SOL Signing Video Test\signing-portrait-2-23 compressed.mp4"



#Top Right
#C:\ffmpeg\bin\ffmpeg.exe -i $FileNameSourceLesson  -i $FileNameSourceSigning -filter_complex "[1:v]scale=500:-1[v2];[0:v][v2]overlay=main_w-overlay_w-5:5" -c:v libx264 -c:a copy  -t 30 $FileNameTopRight

#Bottom Right
#C:\ffmpeg\bin\ffmpeg.exe -i $FileNameSourceLesson  -i $FileNameSourceSigning -filter_complex "[1:v]scale=500:-1[v2];[0:v][v2]overlay=main_w-overlay_w-5:main_h-overlay_h-5" -c:v libx264 -c:a copy  -t 30 $FileNameBottomRight

#Split Equal - no sound
#C:\ffmpeg\bin\ffmpeg.exe  -i $FileNameSourceLesson  -i $FileNameSourceSigning -filter_complex '[0:v]pad=iw*2:ih[int];[int][1:v]overlay=W/2:0[vid]' -map '[vid]'  -c:v libx264  -crf 23 -preset veryfast -t 30 $FileNameSplitEqual
 
#Split Lesson Main  - no sound
#C:\ffmpeg\bin\ffmpeg.exe  -i $FileNameSourceLesson  -i $FileNameSourceSigning -filter_complex "[0:v]scale=960:-1,setsar=1,pad=960:720:(ow-iw)/2:(oh-ih)/2[v0]; [1:v]scale=320:-1,setsar=1,pad=320:720:(ow-iw)/2:(oh-ih)/2[v1]; [v0][v1]hstack[outv]" -map [outv] -c:v libx264 -crf 23 -preset medium -c:a copy -preset veryfast -t 30  $FileNameSplitMainLesson

#Bottom Right Embed - removed alpha (also moves centered persion 300 pixels along)
#C:\ffmpeg\bin\ffmpeg.exe -i $FileNameSourceLesson  -i $FileNameSourceSigning  -filter_complex '[1:v]scale=1000:-1[inputscaled];[inputscaled]chromakey=color=0x2C9738:similarity=0.1[inputchromakey];[0:v][inputchromakey]overlay=main_w-overlay_w--300:main_h-overlay_h-5:eof_action=pass;' -c:v libx264 -c:a copy -preset veryfast -t 30 $FileNameBottomRightEmbed


#Bottom Right Embed Offset - removed alpha (also moves centered persion 300 pixels along)
C:\ffmpeg\bin\ffmpeg.exe  -i $FileNameSourceLesson  -i $FileNameSourceSigningPortrait -filter_complex "color=0xF9F2DF:s=1920x1080:d=10[base]; [0:v]crop=in_w-2:in_h:0:0,scale=1345:-1[v1]; [1:v]crop=in_w-0:in_h:0:0,chromakey=0x2C9738:0.1,scale=-1:810[v2]; [base][v1]overlay=0:(H-overlay_h)/2[tmp]; [tmp][v2]overlay=1345+(575-overlay_w)/2:H-overlay_h[outv]" -map "[outv]" -map 0:a -vcodec h264 -b:a 96k -shortest  -t 30 $FileNameBottomRightEmbedOffset


#Bottom Right Embed Offset - removed alpha (also moves centered persion 300 pixels along) - encrypted
#C:\ffmpeg\bin\ffmpeg.exe  -i $FileNameSourceLesson  -i $FileNameSourceSigningPortrait -filter_complex "color=0xF9F2DF:s=1920x1080:d=10[base]; [0:v]crop=in_w-2:in_h:0:0,scale=1345:-1[v1]; [1:v]crop=in_w-0:in_h:0:0,chromakey=0x2C9738:0.1,scale=-1:810[v2]; [base][v1]overlay=0:(H-overlay_h)/2[tmp]; [tmp][v2]overlay=1345+(575-overlay_w)/2:H-overlay_h[outv]" -map "[outv]" -map 0:a -vcodec h264 -b:a 96k -shortest -start_number 0 -hls_time 10 -hls_list_size 0 -f hls -hls_enc 1 -t 30 $FileNameBottomRightEmbedOffsetEncypted


# use -t 20 