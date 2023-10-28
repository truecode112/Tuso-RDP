# TUSO RDP

![image](https://user-images.githubusercontent.com/92359442/224527446-0574f472-de1b-4069-95d2-06626ce5a193.png)

# Deployment

**Open `Powershel/Terminal` on `Project` Directory. And run this command one by one**

```
pwd
```

```
C:\Users\ETL\Desktop\RDP-Azmin\Tuso-RDP
```

```
$Root ="C:\Users\ETL\Desktop\RDP-Azmin\Tuso-RDP"
```

```
Set-Location -Path $Root
$VersionString = git show -s --format=%ci
$VersionDate = [DateTimeOffset]::Parse($VersionString)
$CurrentVersion = $VersionDate.ToString("yyyy.MM.dd.HHmm")

&"$Root\Utilities\Publish.ps1" -rid win10-x64 -outdir "C:\publish"
```
