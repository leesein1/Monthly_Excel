# Monthly Excel

네이버 카페 글 URL 기반 크롤링 결과를 엑셀로 정리하고, 키워드 분리와 블로그 본문 정리까지 한 번에 처리하는 WinForms 도구입니다.

## 주요 기능

- 카페 글 URL 목록을 읽어서 제목, 조회수, 작성일, 댓글 수를 수집
- 결과를 엑셀 원본 영역 + 조회수 정렬 요약 영역으로 저장
- 키워드/모바일 노출 문구를 규칙에 맞게 분리
- WebView2 기반 블로그 본문 정리
- 블로그 본문 이미지 일괄 다운로드

## 기술 스택

- .NET 8 WinForms
- Selenium WebDriver + ChromeDriver
- ClosedXML
- WebView2

## 실행 방법

1. `Monthly_Excel.sln` 또는 `Monthly_Excel/Monthly_Excel.csproj`를 엽니다.
2. NuGet 패키지를 복원합니다.
3. `Monthly_Excel` 프로젝트를 실행합니다.

## 프로젝트 구조

```text
Monthly_Excel/
├─ Monthly_Excel.sln
├─ README.md
└─ Monthly_Excel/
   ├─ Program.cs
   ├─ Form1.cs
   ├─ Form1.Designer.cs
   ├─ Handlers/
   │  ├─ BlogCleanerHandler.cs
   │  ├─ CrawlingEventHandler.cs
   │  └─ KeywordEventHandler.cs
   ├─ Models/
   │  ├─ CrawlResult.cs
   │  └─ CrawlTarget.cs
   ├─ Processors/
   │  ├─ BlogCleanerProcessor.cs
   │  ├─ CafeArticleCrawler.cs
   │  ├─ CrawlingProcessor.cs
   │  ├─ CrawlWorkbookWriter.cs
   │  └─ KeywordProcessor.cs
   ├─ Scripts/
   │  └─ BlogCleanerScriptProvider.cs
   └─ Utils/
      └─ WebView2TempManager.cs
```

## 탭별 설명

### 1. 크롤링

- 엑셀 양식을 다운로드
- B열부터 URL과 키워드를 입력
- 업로드 후 다운로드를 실행하면 결과 엑셀이 생성

원본 영역:

- 4행 링크
- 5행 키워드
- 6행 글제목
- 7행 조회수
- 8행 작성일
- 9행 댓글수

요약 영역:

- 11행부터 조회수 기준 정렬 결과 작성

### 2. 키워드

- 여러 줄 텍스트를 입력
- `모바일`, `카페` 기준으로 좌/우 리스트 분리
- 각 리스트를 엑셀에 붙여넣기 쉬운 형태로 복사 가능

### 3. 블로그 정리

- 네이버 블로그 URL 열기
- 본문에서 이미지, 링크 카드, 불필요 요소 제거
- 우클릭 제한 완화
- 원본 이미지 수집 후 폴더로 저장

## 현재 구조 방향

- `Form1`은 UI 조립과 이벤트 연결만 담당
- `Handlers`는 탭 단위 사용자 흐름 담당
- `Processors`는 실제 기능 처리 담당
- `Models`는 크롤링 데이터 모델 담당

## 빌드 확인

로컬 기준 `dotnet build Monthly_Excel/Monthly_Excel.csproj --no-restore`로 빌드 확인했습니다.

## 주의 사항

- 카페/블로그 DOM 구조가 바뀌면 선택자 수정이 필요할 수 있습니다.
- Selenium 실행 환경에 Chrome이 정상 설치되어 있어야 합니다.
- 블로그 이미지 다운로드는 원본 서버 응답 상태에 영향을 받습니다.
