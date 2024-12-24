# Changelog

All notable changes to this project will be documented in this file.

## 버전 관리 정책

이 프로젝트는 Semantic Versioning을 따릅니다:

- **Major.Minor.Patch** 형식
  - **Major**: 호환성이 깨지는 변경
  - **Minor**: 하위 호환성 있는 기능 추가
  - **Patch**: 하위 호환성 있는 버그 수정
- **최신 버전이 상단에, 이전 버전이 하단에 기록됩니다.**

## [0.2.0] - 2024-12-13

### Added

- ScriptableObject 기반의 Singleton 구현 (`SingletonScriptableObject<T>`)
  - 스레드 안전한 싱글톤 인스턴스 생성
  - Lazy initialization 지원
  - Resources 폴더 기반 자동 에셋 로드
  - 에디터 환경에서 자동 에셋 생성 지원
  - 경로 검증 및 안전한 에셋 관리
  - 런타임/에디터 환경에서의 에러 처리
  - 커스텀 에셋 경로 설정 기능

## [0.1.0] - 2024-12-12

<!-- markdownlint-disable MD024 -->
### Added
<!-- markdownlint-enable MD024 -->

- Initialize Package
- MonoBehaviour 기반의 Singleton 구현 (`SingletonMonoBehaviour<T>`)
  - 스레드 안전한 싱글톤 인스턴스 생성
  - 씬 전환 시에도 유지되는 싱글톤 (DontDestroyOnLoad 지원)
  - 비활성화된 게임 오브젝트에서 인스턴스 검색 가능
  - 명시적 초기화 지원 (`EnsureInitialized()`)
  - 중복 인스턴스 방지
  - 사용자 정의 초기화 로직 추가 가능 (`OnSingletonAwake()`)

### Changed

### Fixed

## [0.0.1] - 2024-12-12

<!-- markdownlint-disable MD024 -->
### Added
<!-- markdownlint-enable MD024 -->

- Initialize Package
- 일반 C# 클래스 기반의 Singleton 구현 (`Singleton<T>`)
  - 스레드 안전한 싱글톤 인스턴스 생성
  - Lazy initialization 지원
  - `IDisposable` 인터페이스를 통한 리소스 관리 지원
  - 리플렉션을 이용한 생성자 호출 방지
  - 싱글톤 인스턴스 초기화 여부 확인 기능
  - 싱글톤 인스턴스 Dispose 여부 확인 기능
