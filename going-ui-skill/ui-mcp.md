# LauncherTouch — MCP 장치 제어 패턴

Going UI 앱이 배포되는 **라즈베리파이 키오스크/사이니지 장치**를 원격 제어할 때 참조.

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

## GoingTouchCLI (gtcli)

LauncherTouch MCP Server의 모든 기능을 래핑한 CLI 도구.
**Claude Code가 Bash에서 직접 실행하여 장치를 제어**한다.

### 실행 파일 위치

이 스킬 디렉토리에 `gtcli.exe`가 포함되어 있다.
Claude Code에서 실행 시 **이 스킬 파일이 위치한 디렉터리** 기준으로 경로를 지정:

```bash
"./gtcli.exe" scan
"./gtcli.exe" status 172.30.1.13 --token "토큰값"
```

> Self-contained 빌드 (약 15MB)이므로 .NET 런타임 없이 실행 가능.
> 소스: `LauncherTouch/src/GoingTouchCLI/`

### 인증

```bash
# 방법 1: --token 옵션 (권장)
gtcli status 172.30.1.13 --token "토큰값"

# 방법 2: 환경변수
export GTCLI_TOKEN="토큰값"
gtcli status 172.30.1.13
```

토큰은 LauncherTouch 웹 UI(http://{host}:5000)의 **시스템 페이지**에서 확인.

### 전체 명령어

| 분류 | 명령어 | 설명 |
|------|--------|------|
| **검색** | `scan [timeout_ms]` | mDNS로 네트워크의 LauncherTouch 장비 검색 (기본 3초) |
| **시스템** | `status <host>` | 호스트명, 업타임, CPU/메모리 사용량 |
| | `reboot <host>` | 장비 재부팅 |
| | `restart <host>` | LauncherTouch 서비스만 재시작 |
| | `hide-ui <host>` | 태스크바/패널 숨기기 (키오스크 모드) |
| | `show-ui <host>` | 태스크바/패널 표시 |
| | `ui-status <host>` | 태스크바 표시 상태 확인 |
| **프로그램** | `programs <host>` | 설치된 프로그램 목록 |
| | `start <host> <app>` | 프로그램 시작 |
| | `stop <host> <app>` | 프로그램 중지 |
| | `install <host> <zipFileName> <displayName> <executableFileName> [autoStart]` | 프로그램 설치 |
| | `uninstall <host> <app>` | 프로그램 삭제 |
| | `autostart <host> <app> <bool>` | 자동실행 설정 |
| **배포** | `deploy <host> <zipPath> [displayName] [executableFileName] [autoStart]` | 업로드→설치→시작 원클릭 |
| **파일** | `upload <host> <filePath>` | 파일 업로드 |
| | `uploads <host>` | 업로드 파일 목록 |
| | `delete-upload <host> <file>` | 업로드 파일 삭제 |
| **네트워크** | `network <host>` | 네트워크 상태 (IP, 인터페이스) |
| | `static-ip <host> <ip> <sub> <gw> [dns]` | 고정 IP 설정 |
| | `wifi-scan <host>` | Wi-Fi 스캔 |
| | `wifi-connect <host> <ssid> [pw]` | Wi-Fi 연결 |
| **폰트** | `fonts <host>` | 설치된 폰트 목록 |
| | `install-font <host> <file>` | 폰트 설치 (uploads에서) |
| | `remove-font <host> <name>` | 폰트 삭제 |
| | `refresh-fonts <host>` | 폰트 캐시 갱신 |
| **부트이미지** | `boot-image <host> <file>` | 부트 이미지 변경 (uploads에서) |
| | `boot-status <host>` | 현재 부트 이미지 상태 |
| **디버그** | `tools <host>` | MCP Server 등록 도구 목록 (JSON) |

---

## mDNS (네트워크 장치 검색)

LauncherTouch는 mDNS를 내장하여 IP 대신 `{hostname}.local` 호스트명으로 접근 가능.

```bash
# 장비 검색
gtcli scan

# 출력 예시:
# 장비                       IP                 Web      MCP
# ------------------------------------------------------------
# going-01                  172.30.1.13        :5000    :5001
```

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

---

## Claude Code 빌드-배포 전체 흐름

Going UI 프로젝트를 빌드하고 터치 장치에 배포하는 **엔드투엔드 자동화 흐름**.

### 전제 조건
- 사용자가 MCP 토큰을 제공한 상태 (웹 UI 시스템 페이지에서 확인)
- gtcli.exe가 빌드된 상태

### 1단계: 장비 검색 (Bash)

```bash
gtcli scan
```

### 2단계: 빌드 (Bash)

```bash
# linux-arm64 타겟으로 self-contained 빌드
dotnet publish -c Release -r linux-arm64 --self-contained -p:PublishSingleFile=true -o ./publish
```

> 터치 장치는 ARM64 Linux이므로 반드시 `linux-arm64` 런타임 지정.

### 3단계: 압축 (Bash)

```bash
# publish 폴더를 zip으로 압축
cd ./publish && zip -r ../MyApp.zip . && cd ..
```

> Windows에서 작업 시 `Compress-Archive` 또는 설치된 zip 도구 사용.

### 4단계: 배포 — deploy 원클릭 (Bash)

```bash
# deploy = 업로드 → 기존 앱 정리 → 설치 → 시작
gtcli deploy {host} ./MyApp.zip "내 앱" MyApp true --token "{token}"
```

deploy 내부 동작:
1. ZIP 파일을 장비에 업로드
2. 기존 동명 앱 중지 및 삭제
3. 새 앱 설치
4. 앱 시작
5. 업로드된 ZIP 파일 정리

### 5단계: 키오스크 설정 (Bash, 선택)

```bash
gtcli hide-ui {host} --token "{token}"       # 태스크바 숨김
```

> 전체 흐름 요약: `dotnet publish` → `zip` → `gtcli deploy`

---

## 프로그램 업데이트 흐름

이미 설치된 프로그램을 새 버전으로 교체:

```bash
# 1. 빌드 + 압축
dotnet publish -c Release -r linux-arm64 --self-contained -p:PublishSingleFile=true -o ./publish
cd ./publish && zip -r ../MyApp.zip . && cd ..

# 2. 원클릭 배포 (기존 앱 자동 정리)
gtcli deploy {host} ./MyApp.zip "내 앱" MyApp true --token "{token}"
```

---

## 기타 작업별 워크플로우

### 폰트 설치

```bash
gtcli upload {host} ./NanumGothic.ttf --token "{token}"
gtcli install-font {host} NanumGothic.ttf --token "{token}"
gtcli refresh-fonts {host} --token "{token}"
gtcli delete-upload {host} NanumGothic.ttf --token "{token}"    # 선택
```

### 부트 이미지 변경

```bash
gtcli upload {host} ./splash.png --token "{token}"
gtcli boot-image {host} splash.png --token "{token}"
gtcli boot-status {host} --token "{token}"                      # 적용 확인
```

### 네트워크 설정

```bash
gtcli network {host} --token "{token}"                          # 현재 상태 확인
gtcli static-ip {host} 192.168.0.100 255.255.255.0 192.168.0.1 8.8.8.8 --token "{token}"  # 고정 IP
gtcli wifi-scan {host} --token "{token}"                        # Wi-Fi 스캔
gtcli wifi-connect {host} MyWiFi password123 --token "{token}"  # Wi-Fi 연결
```

### 키오스크 모드 전환

```bash
gtcli ui-status {host} --token "{token}"                        # 현재 상태 확인
gtcli hide-ui {host} --token "{token}"                          # 태스크바 숨김
gtcli autostart {host} MyApp true --token "{token}"             # 앱 자동실행 설정
```

### 시스템 점검

```bash
gtcli status {host} --token "{token}"                           # CPU, 메모리, 업타임, IP
gtcli ui-status {host} --token "{token}"                        # 태스크바 상태
gtcli programs {host} --token "{token}"                         # 프로그램 목록
gtcli network {host} --token "{token}"                          # 네트워크 상태
```

---

## 파일 업로드

프로그램 설치, 폰트 설치, 부트 이미지 변경 등 파일이 필요한 작업은 **사전에 `/home/pi/uploads/` 경로에 파일이 존재해야** 함.

### 방법 1: gtcli upload (권장)

```bash
gtcli upload {host} ./MyApp.zip --token "{token}"
```

### 방법 2: Upload API (curl)

인증 없이 HTTP로 파일을 업로드.

```bash
curl -F "file=@MyApp.zip" http://{host}:5000/api/upload
```

### 방법 3: SCP

SSH 접근이 가능한 경우 scp로 직접 전송. (사용자가 직접 실행)

```bash
scp MyApp.zip pi@{host}:/home/pi/uploads/
```

---

## MCP 도구 레퍼런스 (총 25개)

gtcli 명령어가 내부적으로 호출하는 MCP 도구 목록. 직접 MCP 호출이 필요한 경우 참조.

### 1. 프로그램 관리 (ProgramTools) — 6개

| 도구 (snake_case) | gtcli 명령어 | 파라미터 | 설명 |
|------|--------|---------|------|
| `get_programs` | `programs` | 없음 | 설치된 프로그램 목록 |
| `start_program` | `start` | `appName` | 프로그램 시작 |
| `stop_program` | `stop` | `appName` | 프로그램 중지 |
| `install_program` | `install` | `zipFileName`, `displayName`, `executableFileName`, `autoStart` | 프로그램 설치 |
| `uninstall_program` | `uninstall` | `appName` | 프로그램 삭제 |
| `set_auto_start` | `autostart` | `appName`, `autoStart` | 자동실행 설정 |

### 2. 네트워크 관리 (NetworkTools) — 4개

| 도구 (snake_case) | gtcli 명령어 | 파라미터 | 설명 |
|------|--------|---------|------|
| `get_network_status` | `network` | 없음 | 네트워크 상태 |
| `set_static_ip` | `static-ip` | `ip`, `subnet`, `gateway`, `dns` | 고정 IP 설정 |
| `scan_wifi` | `wifi-scan` | 없음 | Wi-Fi 스캔 |
| `connect_wifi` | `wifi-connect` | `ssid`, `password` | Wi-Fi 연결 |

### 3. 폰트 관리 (FontTools) — 4개

| 도구 (snake_case) | gtcli 명령어 | 파라미터 | 설명 |
|------|--------|---------|------|
| `get_fonts` | `fonts` | 없음 | 설치된 폰트 목록 |
| `install_font` | `install-font` | `fontFileName` | 폰트 설치 |
| `remove_font` | `remove-font` | `fontName` | 폰트 삭제 |
| `refresh_font_cache` | `refresh-fonts` | 없음 | fc-cache 실행 |

### 4. 부트 이미지 관리 (BootImageTools) — 2개

| 도구 (snake_case) | gtcli 명령어 | 파라미터 | 설명 |
|------|--------|---------|------|
| `set_boot_image` | `boot-image` | `imageFileName` | 부트 이미지 변경 |
| `get_boot_image_status` | `boot-status` | 없음 | 부트 이미지 상태 |

### 5. 시스템 관리 (SystemTools) — 6개

| 도구 (snake_case) | gtcli 명령어 | 파라미터 | 설명 |
|------|--------|---------|------|
| `get_system_info` | `status` | 없음 | 시스템 정보 |
| `reboot` | `reboot` | `confirm` (필수=true) | 시스템 재부팅 |
| `restart_launcher` | `restart` | 없음 | 서비스 재시작 |
| `hide_desktop_ui` | `hide-ui` | 없음 | 태스크바 숨김 |
| `show_desktop_ui` | `show-ui` | 없음 | 태스크바 표시 |
| `get_desktop_ui_status` | `ui-status` | 없음 | 태스크바 상태 |

### 6. 업로드 파일 관리 (UploadTools) — 3개

| 도구 (snake_case) | gtcli 명령어 | 파라미터 | 설명 |
|------|--------|---------|------|
| `list_uploads` | `uploads` | 없음 | 업로드 파일 목록 |
| `upload_file` | `upload` | `fileName`, `base64Data` | Base64 파일 업로드 (소규모) |
| `delete_upload` | `delete-upload` | `fileName` | 파일 삭제 |

> **MCP 도구 이름 규칙**: C# MCP SDK는 PascalCase 메서드명을 **snake_case**로 자동 변환한다.
> 예: `GetSystemInfo()` → `get_system_info`

---

## 사용자 요청 예시와 대응

| 사용자 요청 | Claude Code 대응 (gtcli) |
|---|---|
| "장비 검색해줘" | `gtcli scan` |
| "설치된 프로그램 보여줘" | `gtcli programs {host} --token {token}` |
| "MyApp 시작해" | `gtcli start {host} MyApp --token {token}` |
| "이 프로젝트 빌드해서 장치에 배포해줘" | `dotnet publish` → `zip` → `gtcli deploy {host} App.zip ...` |
| "이 zip 파일 터치에 설치해줘" | `gtcli deploy {host} {zipPath} {appName} {appName} true --token {token}` |
| "시스템 상태 알려줘" | `gtcli status {host} --token {token}` |
| "Wi-Fi 연결해줘" | `gtcli wifi-scan` → 사용자 SSID 선택 → `gtcli wifi-connect` |
| "부팅 화면 바꿔줘" | `gtcli upload` → `gtcli boot-image` |
| "키오스크 모드로 바꿔" | `gtcli hide-ui` + `gtcli autostart` |
| "네트워크 정보 확인" | `gtcli network {host} --token {token}` |
| "재부팅해줘" | 사용자에게 확인 → `gtcli reboot {host} --token {token}` |
| "프로그램 업데이트해줘" | `dotnet publish` → `zip` → `gtcli deploy` |

---

## 주의 사항

### 반드시 사용자 확인이 필요한 작업

| 작업 | gtcli 명령어 | 이유 |
|------|------|------|
| 시스템 재부팅 | `reboot` | 모든 프로그램 종료됨 |
| 프로그램 삭제 | `uninstall` | 되돌릴 수 없음 |
| 고정 IP 설정 | `static-ip` | 네트워크 연결 변경/끊김 가능 |
| 서비스 재시작 | `restart` | MCP 연결 끊김 |

### AppName vs Name

- `AppName`: 디렉터리 이름 기반 식별자 — **도구 호출 시 사용**
- `Name`: UI 표시용 이름
- AppName = zipFileName에서 `.zip` 제거한 값 (예: `MyApp.zip` → `MyApp`)
- 항상 `gtcli programs`로 정확한 `AppName` 확인 후 호출

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
| 장비 연결 실패 | `[ERROR] 연결 실패: ...` |

### 배포 트러블슈팅

| 증상 | 원인 | 해결 |
|------|------|------|
| `401 Unauthorized` | 토큰 누락 또는 만료 | 웹 UI(http://{host}:5000) 시스템 페이지에서 토큰 재확인 |
| `연결 실패` (포트 5001) | 방화벽 차단 또는 LauncherTouch 미시작 | `gtcli scan`으로 장비 검색 → 응답 없으면 장비 네트워크/전원 확인 |
| 설치 후 앱 실행 안 됨 | zip 내부에 실행파일이 루트에 없음 | `cd ./publish && zip -r ../App.zip .` — publish **폴더 안에서** 압축해야 함 |
| `프로그램을 찾을 수 없습니다` | AppName 불일치 | `gtcli programs`로 정확한 AppName 확인 후 재시도 |
| 업로드 성공 but 설치 실패 | zip 파일 손상 또는 빈 파일 | 로컬에서 zip 해제 테스트 후 재압축 |

---

## 기술 세부사항

### MCP 프로토콜

- **전송**: Streamable HTTP (POST http://{host}:5001/mcp)
- **메시지 형식**: JSON-RPC 2.0
- **응답 형식**: SSE (Server-Sent Events) 또는 JSON
- **인증**: `X-MCP-Token` 헤더
- **세션**: `Mcp-Session-Id` 헤더로 세션 유지

### 파일 업로드 (REST API)

- **엔드포인트**: POST http://{host}:5000/api/upload
- **형식**: multipart/form-data
- **인증**: 불필요
- **최대 크기**: 500MB
- **저장 경로**: /home/pi/uploads/

### GoingTouchCLI 프로젝트 구조

```
LauncherTouch/src/
├── LauncherTouch.sln
├── LauncherTouch/           # LauncherTouch 서버 (Blazor + MCP Server)
│   ├── LauncherTouch.csproj
│   ├── Program.cs
│   ├── Components/
│   ├── Services/
│   ├── Mcp/
│   └── ...
└── GoingTouchCLI/           # CLI 도구
    ├── GoingTouchCLI.csproj
    ├── Program.cs            # CLI 엔트리포인트, 모든 명령어 라우팅
    ├── McpClient.cs          # MCP Streamable HTTP 클라이언트
    └── MdnsScanner.cs        # mDNS 장비 검색
```
