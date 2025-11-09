# ===== ????? ??????? FTP =====
$ftpServer = "ftp://site42218.siteasp.net/"   # ?????? ??????
$username = "site42218"
$password = "c#4QC7t@-W8r"
$localPath = "C:\Users\skynet\source\repos\WebApplication18\WebApplication18\wwwroot"  # ?????? ??????

# ===== ???? ??? ??????? ????????? =====
function Upload-Folder($localFolder, $remoteFolder) {
    # ?????? ????? ?????? ?????? (??? ?? ??? ?????)
    try {
        $request = [System.Net.FtpWebRequest]::Create($remoteFolder)
        $request.Credentials = New-Object System.Net.NetworkCredential($username,$password)
        $request.Method = [System.Net.WebRequestMethods+Ftp]::MakeDirectory
        $request.GetResponse().Close()
    } catch {
        # ????? ????? ??? ??? ?????? ????? ??????
    }

    # ??? ??????? ???? ?????? ??????
    Get-ChildItem $localFolder -File | ForEach-Object {
        $file = $_.FullName
        $uri = New-Object System.Uri($remoteFolder + $_.Name)
        $ftp = [System.Net.FtpWebRequest]::Create($uri)
        $ftp.Credentials = New-Object System.Net.NetworkCredential($username,$password)
        $ftp.Method = [System.Net.WebRequestMethods+Ftp]::UploadFile
        $ftp.UseBinary = $true
        $ftp.UsePassive = $true
        $content = [System.IO.File]::ReadAllBytes($file)
        $ftp.ContentLength = $content.Length
        $requestStream = $ftp.GetRequestStream()
        $requestStream.Write($content,0,$content.Length)
        $requestStream.Close()
        $response = $ftp.GetResponse()
        $response.Close()
        Write-Host "Uploaded file:" $_.FullName
    }

    # ??? ???????? ???????
    Get-ChildItem $localFolder -Directory | ForEach-Object {
        $subLocal = $_.FullName
        $subRemote = $remoteFolder + $_.Name + "/"
        Upload-Folder $subLocal $subRemote
    }
}

# ===== ??? ??? ??????? =====
Upload-Folder $localPath $ftpServer
Write-Host "All files uploaded successfully!"
