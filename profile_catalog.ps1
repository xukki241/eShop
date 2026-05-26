$pid = 22968
$port = 62499
$base = "http://localhost:$port/api/catalog"
$out = "catalog_final"

# Chạy trace trong background
$traceJob = Start-Job -ScriptBlock {
    param($p, $o)
    & dotnet-trace collect -p $p --profile dotnet-sampled-thread-time --duration 00:00:30 --format Speedscope -o "$o.nettrace"
} -ArgumentList $pid, "D:\OJT\RikkeiSoft\DotNet\eShop\$out"

Start-Sleep -Seconds 2

# Gọi API liên tục trong 25 giây để tạo CPU load
$end = (Get-Date).AddSeconds(25)
$i = 1
while ((Get-Date) -lt $end) {
    Invoke-WebRequest "$base/items?pageSize=20&pageIndex=0&api-version=1.0" -UseBasicParsing -ErrorAction SilentlyContinue | Out-Null
    Invoke-WebRequest "$base/items/$i`?api-version=1.0" -UseBasicParsing -ErrorAction SilentlyContinue | Out-Null
    Invoke-WebRequest "$base/items/by?ids=1,2,3,4,5&api-version=1.0" -UseBasicParsing -ErrorAction SilentlyContinue | Out-Null
    Invoke-WebRequest "$base/catalogtypes?api-version=1.0" -UseBasicParsing -ErrorAction SilentlyContinue | Out-Null
    Invoke-WebRequest "$base/catalogbrands?api-version=1.0" -UseBasicParsing -ErrorAction SilentlyContinue | Out-Null
    Invoke-WebRequest "$base/items/by/Shirt?pageSize=10&api-version=1.0" -UseBasicParsing -ErrorAction SilentlyContinue | Out-Null
    Invoke-WebRequest "$base/items/type/1/brand/1?api-version=1.0" -UseBasicParsing -ErrorAction SilentlyContinue | Out-Null
    $i = ($i % 10) + 1
    Write-Host "Loop $i - $(Get-Date -Format 'HH:mm:ss')"
}

# Chờ trace job hoàn thành
Write-Host "Waiting for trace to complete..."
Wait-Job $traceJob
Receive-Job $traceJob
Remove-Job $traceJob

Write-Host ""
Write-Host "Done! Files created:"
Get-ChildItem "D:\OJT\RikkeiSoft\DotNet\eShop\$out*"
