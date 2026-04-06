# Monthly Excel Manager

![.NET](https://img.shields.io/badge/.NET-8.0-purple)
![C#](https://img.shields.io/badge/C%23-WinForms-blue)
![Version](https://img.shields.io/badge/version-1.2.3-2b7a78)
![License](https://img.shields.io/badge/License-MIT-green)

> 반복 업무를 줄이기 위해 만든 **WinForms 기반 데스크톱 자동화 도구**입니다.

`Monthly Excel Manager`는  
네이버 카페 URL 기반 데이터 수집부터 블로그 정리, 키워드 정리, 이미지 변환까지  
실제 업무 흐름을 줄이기 위해 만든 **실사용 자동화 프로젝트**입니다.

현재는 **5개의 주요 기능 탭**을 중심으로 동작합니다.

- 크롤링
- 키워드 정리
- 블로그 정리
- 이미지 변환
- 검사기

---

# Why I Built This

여자친구 회사에서는 네이버 카페 게시글을 하나씩 열어 보고

- 조회수
- 댓글 수
- 작성일

같은 정보를 확인한 뒤 다시 **엑셀에 직접 입력하는 작업**을 반복하고 있었습니다.

이 작업은 반복될수록

- 시간이 오래 걸리고
- 입력 실수가 발생하며
- 피로도가 높아지는 문제

가 있었습니다.

그래서 **URL만으로 데이터를 수집하고 엑셀로 정리하는 자동화 도구**를 만들게 되었습니다.

---

# Overview

이 프로젝트는 처음에는 **카페 URL 크롤링 자동화**로 시작했지만,  
실제 사용 흐름에 맞춰 기능이 확장되었습니다.

- 블로그 본문 정리
- 이미지 다운로드 및 변환
- 키워드 정리

현재 버전은 `v1.2.3`이며, 다음과 같은 개선이 반영되어 있습니다.

- 구조 리팩토링 (Pages / Handlers / Processors 분리)
- UI 스타일 통일
- 로딩 UX 개선
- WebView2 캐시 관리
- 이미지 미리보기 성능 개선
- 검사기 탭 확장 (글자수 + 번역기)

---

# Features

## 1. 크롤링

엑셀에 정리된 URL 목록을 기반으로 게시글 정보를 수집합니다.

- 엑셀 업로드
- 결과 엑셀 다운로드
- 템플릿 엑셀 다운로드

---

## 2. 키워드 정리

복사한 키워드를 자동으로 분리하고 리스트 형태로 정리합니다.

- 키워드 자동 분리
- 좌/우 리스트 구성
- 결과 복사 기능

---

## 3. 블로그 정리

WebView2 기반으로 블로그를 열고 본문을 정리하거나 이미지를 다운로드합니다.

- URL 열기 (Enter 지원)
- 본문 정리
- 새로고침
- 이미지 다운로드
- 열기 전 캐시 초기화

---

## 4. 이미지 변환

이미지를 선택하고 포맷을 일괄 변환할 수 있습니다.

- 폴더 선택
- 이미지 목록 / 선택 목록 분리
- 미리보기
- 일괄 변환

---

## 5. 검사기

글자수 확인과 번역 작업을 한 화면에서 빠르게 전환할 수 있습니다.

- 사람인 글자수 도구 연동
- Papago 번역기 연동
- 라디오 버튼 전환
- 도구 선로드로 빠른 화면 전환

---

# Tech Stack

- .NET 8
- C#
- WinForms
- WebView2
- Selenium WebDriver
- ChromeDriver
- ClosedXML
- SixLabors.ImageSharp

---

# Project Structure

```
Monthly_Excel/
├─ Controls/
├─ Handlers/
├─ Models/
├─ Pages/
├─ Processors/
├─ Scripts/
├─ UI/
└─ Utils/
```

---

# Architecture

- Pages: UI 구성
- Handlers: 이벤트 처리
- Processors: 핵심 로직
- UI: 테마/스타일
- Controls: 공통 컴포넌트
- Utils: 유틸

---

# Performance Notes

## WebView2
- 선초기화
- 캐시 분리 및 삭제
- 세션 폴더 관리

## 이미지 처리
- 미리보기 캐시
- 디바운스 적용

## UI
- 로딩 오버레이
- 원형 로더
- 공통 테마 적용

---

# Build

```
dotnet build Monthly_Excel/Monthly_Excel.csproj
```

---

# Run

```
dotnet run --project Monthly_Excel/Monthly_Excel.csproj
```

---

# Distribution

포터블 방식으로 배포합니다.

```
bin/Release/net8.0-windows/
```

Github Releases 배포용 소스 생성 스크립트는 아래처럼 실행합니다.

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\Publish-ReleaseAssets.ps1 `
  -Version "v1.2.3" `
  -Owner "leesein1" `
  -Repository "Monthly_Excel"
```

이미 `Release` 빌드가 끝난 상태라면 `-SkipBuild`를 추가하면 됩니다.

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\Publish-ReleaseAssets.ps1 `
  -Version "v1.2.3" `
  -Owner "leesein1" `
  -Repository "Monthly_Excel" `
  -SkipBuild
```

생성 결과물은 `.artifacts/releases/<tag>/upload` 아래에 만들어지며, `manifest.json`, `app__*`, `launcher__*`, 포터블 zip을 GitHub Release assets에 업로드하면 됩니다.

---

# Notes

- DOM 변경 시 크롤링 수정 필요
- WebView2 런타임 필요
- ChromeDriver 필요

---

# License

MIT License

---

# Patch Notes

## 2026-03-22 (v1.1.0)
- 프로젝트 초기 작성 및 기본 탭 구성 완료
- 크롤링 / 키워드 / 블로그 정리 / 이미지 변환 기능 정리
- Pages / Handlers / Processors 구조 기반 리팩토링 반영

## 2026-03-29 (v1.2.0)
- `맞춤법` 탭 추가 (WebView2 기반 사람인 맞춤법 페이지 연동)
- 맞춤법 화면 불필요 영역 제거 및 표시 영역/스크롤 동작 보정
- 맞춤법 배율 조절 기능 개선 (숫자 입력 + 마우스 휠)
- 배율 저장 체크 옵션 추가 및 설정 저장(JSON) 연동
- `설정` 탭 추가
- 탭 표시/숨김, 순서 변경(버튼 + 드래그 + 키보드) 기능 추가
- 모든 설정 초기화 기능 추가
- 탭/설정 상태 재실행 유지 기능 강화 (로컬 설정 파일 저장/로드)

## 2026-03-29 (v1.2.1)
- 크롤링 실행 중 UI 멈춤 현상 완화: Selenium 동기 구간을 백그라운드 워커에서 수행하도록 변경
- 크롤링 취소 기능 추가: 실행 중 다운로드 버튼을 다시 누르면 취소 요청 처리
- 중복 실행 방지 및 버튼 상태 제어 추가 (업로드/템플릿 버튼 비활성화)
- 진행률 표시 개선: 완료 건수 기반 진행률(%) 및 상태 문구 실시간 갱신
- 안정성 보강: `CancellationToken` 전파, 폼 종료 시 진행 중 크롤링 취소 요청
- 과부하 완화: 크롤링 동시 워커 수 상한을 2로 제한

## 2026-03-31 (v1.2.2)
- 블로그 탭 초기 진입 시 `WebView2` 재초기화 충돌 오류(`already initialized with a different CoreWebView2Environment`) 대응
- 블로그 탭 검은 화면(about:blank) 문제 수정: 초기화 완료 후 기본 블로그 페이지 자동 로드
- 블로그 정리/새로고침/이미지 다운로드 동작 안정화: CoreWebView2 미준비 상태에서 재초기화 후 실행하도록 보강
- 정리 버튼 타이밍 개선: 본문 DOM 로드 확인 후 정리 스크립트 실행하도록 변경
- 상태 메시지 정리: 초기화 실패 상태가 `준비 완료`로 덮어써지지 않도록 수정

## 2026-04-06 (v1.2.3)
- 맞춤법 전용 탭을 `검사기` 탭으로 확장
- 글자수 확인 외에 번역 작업도 자주 필요해 `Papago` 번역기 추가
- 글자수 / 번역기 라디오 버튼 전환 UI 추가
- 두 도구를 미리 로드하도록 변경해 전환 시 재로딩 없이 빠르게 표시되도록 개선
- 관련 페이지/클래스 명칭을 `Inspector` 기준으로 정리
