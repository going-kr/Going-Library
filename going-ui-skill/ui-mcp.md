# LauncherTouch — MCP 장치 제어 패턴

Going UI 앱이 배포되는 **라즈베리파이 키오스크/사이니지 장치**를 MCP Server를 통해 원격 제어할 때 참조.

---

## 장치 개요

| 항목 | 값 |
|------|-----|
| 장치 | Raspberry Pi (ARM64, Debian 13 trixie) |
| 역할 | 키오스크/디지털 사이니지 런처 — 프로그램 관리, 부팅 화면, 폰트, 네트워크 제어 |
| 웹 UI | 포트 5000 (Blazor Server + MudBlazor) |
| MCP Server | 포트 5001 (Streamable HTTP, 토큰 인증) |
| mDNS | `{hostname}.local` 로 장치 접근 |
| 프로그램 설치 경로 | `/home/pi/App/{AppName}/` |
| 파일 업로드 경로 | `/home/pi/uploads/` |

---

## mDNS (네트워크 장치 검색)

LauncherTouch는 mDNS를 내장하여 IP 대신 `{hostname}.local` 호스트명으로 접근 가능.

| 서비스 | 주소 |
|--------|------|
| 웹 UI | `http://{hostname}.local:5000` |
| MCP Server | `http://{hostname}.local:5001/mcp` |
| Upload API | `http://{hostname}.local:5000/api/upload` |

### 호스트명 설정
- **기본값**: OS 호스트명 (보통 `pi`)
- **커스텀**: `appsettings.json`의 `Mdns:Hostname` 또는 웹 UI에서 변경
- **즉시 적용**: 저장 시 mDNS 자동 재시작 (앱 재시작 불필요)
- **규칙**: 영문, 숫자, 하이픈(`-`)만 사용

### Claude Code MCP 설정

사용자가 MCP 토큰을 전달하면 아래 형식으로 MCP 서버에 연결:

```json
{
  "mcpServers": {
    "launchertouch": {
      "type": "streamableHttp",
      "url": "http://{hostname}.local:5001/mcp",
      "headers": {
        "X-MCP-Token": "{토큰값}"
      }
    }
  }
}
```

> mDNS가 작동하지 않는 환경(일부 Windows 네트워크, 서브넷 분리 등)에서는 IP 주소를 직접 사용.

---

## 파일 업로드

프로그램 설치, 폰트 설치, 부트 이미지 변경 등 파일이 필요한 작업은 **사전에 `/home/pi/uploads/` 경로에 파일이 존재해야** 함.

### 방법 1: Upload API (권장 — Claude Code 자동화에 최적)

인증 없이 HTTP로 파일을 업로드. **Claude Code가 Bash에서 `curl`로 직접 실행 가능.**

```bash
# 단일 파일 업로드
curl -F "file=@MyApp.zip" http://{hostname}.local:5000/api/upload

# 여러 파일 동시 업로드
curl -F "file=@app1.zip" -F "file=@app2.zip" http://{hostname}.local:5000/api/upload
```

응답:
```json
{ "uploaded": [{ "fileName": "MyApp.zip", "sizeKB": 1024 }] }
```

### 방법 2: MCP UploadFile 도구

MCP를 통해 Base64 인코딩된 파일 데이터를 직접 전송. **소규모 파일(수 MB 이내)에 적합.**

```
UploadFile(fileName: "config.json", base64Data: "{base64 인코딩 데이터}")
```

### 방법 3: SCP

SSH 접근이 가능한 경우 scp로 직접 전송. (사용자가 직접 실행)

```bash
scp MyApp.zip pi@{DEVICE_IP}:/home/pi/uploads/
```

> `ListUploads` 도구로 업로드된 파일 확인 가능.

---

## MCP 도구 (총 25개)

### 1. 프로그램 관리 (ProgramTools) — 6개

| 도구 | 파라미터 | 설명 |
|------|---------|------|
| `GetPrograms` | 없음 | 설치된 프로그램 목록 `[{ Id, Name, AppName, ExecutableFileName, AutoStart, IsRunning }]` |
| `StartProgram` | `appName` (string) | 프로그램 시작. 이미 실행 중이면 안내 메시지 반환 |
| `StopProgram` | `appName` (string) | 프로그램 중지 |
| `InstallProgram` | `zipFileName`, `displayName`, `executableFileName`, `autoStart`(bool, 기본false) | uploads의 zip → `/home/pi/App/{AppName}/`에 설치. AppName은 zipFileName에서 확장자 제거한 값 |
| `UninstallProgram` | `appName` (string) | 프로그램 삭제 (실행 중이면 먼저 Stop 권장) |
| `SetAutoStart` | `appName` (string), `autoStart` (bool) | 자동실행 설정 |

### 2. 네트워크 관리 (NetworkTools) — 4개

| 도구 | 파라미터 | 설명 |
|------|---------|------|
| `GetNetworkStatus` | 없음 | `{ EthernetIp, WifiIp, Interfaces }` |
| `SetStaticIp` | `ip`, `subnet`, `gateway`, `dns`(선택, 기본 "8.8.8.8") | eth0 고정 IP 설정. **네트워크 끊김 가능 — 사용자 확인 필수** |
| `ScanWifi` | 없음 | `[{ SSID, Signal, IsEncrypted, IsConnected }]` |
| `ConnectWifi` | `ssid`, `password`(선택, 기본 "") | Wi-Fi 연결 |

### 3. 폰트 관리 (FontTools) — 4개

| 도구 | 파라미터 | 설명 |
|------|---------|------|
| `GetFonts` | 없음 | 설치된 폰트 파일명 목록 |
| `InstallFont` | `fontFileName` (string) | uploads의 폰트 설치 (.ttf/.otf/.woff/.woff2) |
| `RemoveFont` | `fontName` (string) | 폰트 삭제 |
| `RefreshFontCache` | 없음 | fc-cache 실행 (폰트 설치/삭제 후 권장) |

### 4. 부트 이미지 관리 (BootImageTools) — 2개

| 도구 | 파라미터 | 설명 |
|------|---------|------|
| `SetBootImage` | `imageFileName` (string) | uploads의 이미지로 Plymouth 부트 화면 변경 (.png/.jpg/.jpeg) |
| `GetBootImageStatus` | 없음 | `{ HasImage, ImagePath }` |

### 5. 시스템 관리 (SystemTools) — 6개

| 도구 | 파라미터 | 설명 |
|------|---------|------|
| `GetSystemInfo` | 없음 | `{ Hostname, OsVersion, Uptime, CpuPercent, MemoryTotalMB, MemoryUsedMB, IpAddress }` |
| `Reboot` | `confirm` (bool, 필수=true) | 시스템 재부팅. **사용자 확인 필수** |
| `RestartLauncher` | 없음 | LauncherTouch 서비스 재시작 (MCP 연결 잠시 끊김) |
| `HideDesktopUi` | 없음 | 태스크바 숨김 (키오스크 모드) |
| `ShowDesktopUi` | 없음 | 태스크바 표시 |
| `GetDesktopUiStatus` | 없음 | `{ IsVisible }` |

### 6. 업로드 파일 관리 (UploadTools) — 3개

| 도구 | 파라미터 | 설명 |
|------|---------|------|
| `ListUploads` | 없음 | uploads 폴더 파일 목록 `[{ FileName, SizeKB, CreatedAt }]` |
| `UploadFile` | `fileName` (string), `base64Data` (string) | Base64 파일 데이터를 uploads에 저장. 소규모 파일(수 MB 이내) 전용. 대용량은 Upload API 사용 |
| `DeleteUpload` | `fileName` (string) | uploads 파일 삭제 |

---

## Claude Code 빌드-배포 전체 흐름

Going UI 프로젝트를 빌드하고 터치 장치에 배포하는 **엔드투엔드 자동화 흐름**.
Claude Code가 MCP 서버에 연결된 상태에서 아래 순서를 따른다.

### 전제 조건
- 사용자가 장치 호스트명(또는 IP)과 MCP 토큰을 제공한 상태
- MCP 서버가 Claude Code에 연결된 상태
- 프로젝트가 빌드 가능한 상태 (`dotnet publish` 성공)

### 1단계: 빌드 (Bash)

```bash
# linux-arm64 타겟으로 self-contained 빌드
dotnet publish -c Release -r linux-arm64 --self-contained -o ./publish
```

> 터치 장치는 ARM64 Linux이므로 반드시 `linux-arm64` 런타임 지정.

### 2단계: 압축 (Bash)

```bash
# publish 폴더를 zip으로 압축
cd ./publish && zip -r ../MyApp.zip . && cd ..
```

> Windows에서 작업 시 `Compress-Archive` 또는 설치된 zip 도구 사용.

### 3단계: 업로드 (Bash — Upload API)

```bash
# curl로 장치에 직접 업로드 (인증 불필요)
curl -F "file=@MyApp.zip" http://{hostname}.local:5000/api/upload
```

> 업로드 성공 시 `{ "uploaded": [{ "fileName": "MyApp.zip", "sizeKB": ... }] }` 응답.

### 4단계: 설치 (MCP)

```
# 기존 프로그램이 있으면 중지 → 삭제 → 재설치
GetPrograms()                          # 기존 설치 확인
StopProgram("MyApp")                   # 실행 중이면 중지
UninstallProgram("MyApp")              # 기존 버전 삭제
InstallProgram("MyApp.zip", "내 앱", "MyApp", true)  # 새 버전 설치 + 자동실행
```

> `InstallProgram`의 `executableFileName`은 확장자 없이 실행 파일명만 지정.
> AppName은 `zipFileName`에서 `.zip`을 제거한 값이 됨 (예: `MyApp.zip` → AppName: `MyApp`).

### 5단계: 실행 및 확인 (MCP)

```
StartProgram("MyApp")                  # 프로그램 시작
GetPrograms()                          # IsRunning 확인
DeleteUpload("MyApp.zip")              # (선택) 설치 파일 정리
```

### 6단계: 키오스크 설정 (MCP, 선택)

```
HideDesktopUi()                        # 태스크바 숨김
SetAutoStart("MyApp", true)            # 부팅 시 자동 실행
```

> 전체 흐름 요약: `dotnet publish` → `zip` → `curl 업로드` → `InstallProgram` → `StartProgram`

---

## 프로그램 업데이트 흐름

이미 설치된 프로그램을 새 버전으로 교체:

```
1. dotnet publish → zip 압축 → curl 업로드        (Bash)
2. StopProgram("MyApp")                            (MCP) 기존 앱 중지
3. UninstallProgram("MyApp")                       (MCP) 기존 앱 삭제
4. InstallProgram("MyApp.zip", "내 앱", "MyApp", true)  (MCP) 새 버전 설치
5. StartProgram("MyApp")                           (MCP) 재시작
6. DeleteUpload("MyApp.zip")                       (MCP) 설치 파일 정리
```

---

## 기타 작업별 워크플로우

### 폰트 설치

```
1. curl -F "file=@NanumGothic.ttf" http://{host}:5000/api/upload   (Bash)
2. InstallFont("NanumGothic.ttf")                                   (MCP)
3. RefreshFontCache()                                                (MCP)
4. DeleteUpload("NanumGothic.ttf")                                   (MCP, 선택)
```

### 부트 이미지 변경

```
1. curl -F "file=@splash.png" http://{host}:5000/api/upload         (Bash)
2. SetBootImage("splash.png")                                        (MCP)
3. GetBootImageStatus()                                              (MCP) 적용 확인
```

### 네트워크 설정

```
1. GetNetworkStatus()                    현재 상태 확인
2. 고정 IP: SetStaticIp(ip, subnet, gateway, dns) — ⚠ 사용자 확인 필수
3. Wi-Fi: ScanWifi() → 사용자 SSID 선택 → ConnectWifi(ssid, password)
```

### 키오스크 모드 전환

```
1. GetDesktopUiStatus()                  현재 상태 확인
2. HideDesktopUi()                       태스크바 숨김
3. SetAutoStart("MyApp", true)           앱 자동실행 설정
4. (필요 시) Reboot(confirm: true)       재부팅 — ⚠ 사용자 확인 필수
```

### 시스템 점검

```
1. GetSystemInfo()                       CPU, 메모리, 업타임, IP 확인
2. GetDesktopUiStatus()                  태스크바 상태
3. GetPrograms()                         프로그램 목록 및 실행 상태
4. GetNetworkStatus()                    네트워크 상태
```

---

## 사용자 요청 예시와 대응

| 사용자 요청 | Claude Code 대응 |
|---|---|
| "설치된 프로그램 보여줘" | `GetPrograms` |
| "MyApp 시작해" | `GetPrograms` → `StartProgram("MyApp")` |
| "모든 프로그램 중지해" | `GetPrograms` → 실행 중인 것들에 대해 각각 `StopProgram` |
| "이 프로젝트 빌드해서 장치에 배포해줘" | `dotnet publish` → `zip` → `curl 업로드` → `InstallProgram` → `StartProgram` |
| "시스템 상태 알려줘" | `GetSystemInfo` |
| "Wi-Fi 연결해줘" | `ScanWifi` → 사용자에게 SSID 선택 요청 → `ConnectWifi` |
| "부팅 화면 바꿔줘" | `ListUploads` → 이미지 파일 확인 → `SetBootImage` |
| "키오스크 모드로 바꿔" | `HideDesktopUi` + `SetAutoStart` |
| "네트워크 정보 확인" | `GetNetworkStatus` |
| "업로드된 파일 정리해줘" | `ListUploads` → 사용자 확인 → `DeleteUpload` |
| "재부팅해줘" | 사용자에게 확인 → `Reboot(confirm: true)` |
| "프로그램 업데이트해줘" | `StopProgram` → `UninstallProgram` → 빌드/업로드 → `InstallProgram` → `StartProgram` |

---

## 주의 사항

### 반드시 사용자 확인이 필요한 작업

| 작업 | 도구 | 이유 |
|------|------|------|
| 시스템 재부팅 | `Reboot` | 모든 프로그램 종료됨 |
| 프로그램 삭제 | `UninstallProgram` | 되돌릴 수 없음 |
| 고정 IP 설정 | `SetStaticIp` | 네트워크 연결 변경/끊김 가능 |
| 서비스 재시작 | `RestartLauncher` | MCP 연결 끊김 |

### AppName vs Name

- `AppName`: 디렉터리 이름 기반 식별자 — **도구 호출 시 사용**
- `Name`: UI 표시용 이름
- AppName = zipFileName에서 `.zip` 제거한 값 (예: `MyApp.zip` → `MyApp`)
- 항상 `GetPrograms`로 정확한 `AppName` 확인 후 호출

### 파일 형식 제한

| 용도 | 허용 형식 |
|------|----------|
| 프로그램 설치 | `.zip` |
| 폰트 설치 | `.ttf`, `.otf`, `.woff`, `.woff2` |
| 부트 이미지 | `.png`, `.jpg`, `.jpeg` |

### 에러 케이스

| 상황 | 결과 |
|------|------|
| 존재하지 않는 AppName | 예외: "프로그램을 찾을 수 없습니다" |
| 지원하지 않는 파일 형식 | 예외: 지원 형식 안내 |
| uploads에 없는 파일 | FileNotFoundException |
| 경로 탈출 시도 (`../`) | SecurityException |
| Upload API 실패 | HTTP 응답 코드 확인, 장치 접근 가능 여부 점검 |
