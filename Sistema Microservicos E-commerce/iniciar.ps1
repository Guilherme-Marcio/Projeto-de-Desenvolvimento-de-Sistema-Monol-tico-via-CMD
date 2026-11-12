# Compila a solucao e sobe cada microservico numa janela propria, depois abre o cliente CMD.
# Uso: powershell -ExecutionPolicy Bypass -File iniciar.ps1

$ErrorActionPreference = "Stop"
$raiz = $PSScriptRoot
$config = "Debug"

Write-Host "Compilando a solucao..." -ForegroundColor Cyan
dotnet build "$raiz\ArteSuave.Microservicos.sln" -c $config --nologo -v q
if ($LASTEXITCODE -ne 0) { Write-Host "Falha na compilacao." -ForegroundColor Red; exit 1 }

function Exe($projeto, $nome) { Join-Path $raiz "$projeto\bin\$config\net472\$nome.exe" }

$servicos = @(
    @{ Nome = "Catalogo";   Exe = (Exe "Servico.Catalogo"   "Servico.Catalogo") },
    @{ Nome = "Vendas";     Exe = (Exe "Servico.Vendas"     "Servico.Vendas") },
    @{ Nome = "Relatorios"; Exe = (Exe "Servico.Relatorios" "Servico.Relatorios") },
    @{ Nome = "Gateway";    Exe = (Exe "Gateway"            "Gateway") }
)

foreach ($s in $servicos) {
    Write-Host "Subindo $($s.Nome)..." -ForegroundColor Green
    Start-Process -FilePath $s.Exe -WorkingDirectory (Split-Path $s.Exe)
}

Start-Sleep -Seconds 2
Write-Host "Abrindo o cliente CMD..." -ForegroundColor Cyan
$cliente = Exe "Cliente.Cmd" "ArteSuave.Cliente"
Start-Process -FilePath $cliente -WorkingDirectory (Split-Path $cliente)
