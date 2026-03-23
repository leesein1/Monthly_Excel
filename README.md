# Monthly Excel Manager

![.NET](https://img.shields.io/badge/.NET-8.0-purple)
![C#](https://img.shields.io/badge/C%23-WinForms-blue)
![Version](https://img.shields.io/badge/version-1.1.0-2b7a78)
![License](https://img.shields.io/badge/License-MIT-green)

> 반복 업무를 줄이기 위해 만든 **WinForms 기반 데스크톱 자동화 도구**입니다.

`Monthly Excel Manager`는  
네이버 카페 URL 기반 데이터 수집부터 블로그 정리, 키워드 정리, 이미지 변환까지  
실제 업무 흐름을 줄이기 위해 만든 **실사용 자동화 프로젝트**입니다.

현재는 **4개의 주요 기능 탭**을 중심으로 동작합니다.

- 크롤링
- 키워드 정리
- 블로그 정리
- 이미지 변환

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

현재 버전은 `v1.1.0`이며, 다음과 같은 개선이 반영되어 있습니다.

- 구조 리팩토링 (Pages / Handlers / Processors 분리)
- UI 스타일 통일
- 로딩 UX 개선
- WebView2 캐시 관리
- 이미지 미리보기 성능 개선

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

---

# Notes

- DOM 변경 시 크롤링 수정 필요
- WebView2 런타임 필요
- ChromeDriver 필요

---

# License

MIT License
