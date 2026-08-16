<div align="center">
  <h1>🪄 Magic - 턴제 헥사곤 던전 크롤러 & 제스처 드로잉 액션 RPG</h1>
  <p><strong>마우스로 직접 마법진(도형)을 그리고 조합하여, 전략적으로 헥사곤 던전을 돌파하는 체감형 판타지 어드벤처</strong></p>

  <!-- 방패 뱃지들 -->
  <img src="https://img.shields.io/badge/Unity-100000?style=for-the-badge&logo=unity&logoColor=white" alt="Unity">
  <img src="https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=c-sharp&logoColor=white" alt="C#">
  <img src="https://img.shields.io/badge/AI_Used-10B981?style=for-the-badge" alt="AI 사용">
  <br><br>
</div>

## 📌 Project Overview
- **개발 기간:** 2026.07.01 ~ 2026.08.29
- **개발 인원:** 1인 개발 (프로그래밍 전담 / 아트 리소스는 AI 활용)
- **엔진 버전:** Unity 6 (6000.5.4f1)
- **장르:** 상점 경영, 턴제 헥사곤 던전 크롤러, 제스처 액션 RPG
- **AI 활용 (Antigravity & Generative AI):** 
  - **프로그래밍:** PointCloud Recognition 알고리즘 최적화 및 수학적 예외 처리 로직 디버깅에 Antigravity 활용
  - **아트:** 미드저니, DALL-E 등 이미지 생성 AI를 적극 활용하여 인게임 UI, 마법 아이콘, 헥사곤 타일맵 텍스처 등 제작

## 🎮 Game Concept
마법 스크롤을 직접 제작하여 판매하는 상점을 운영하고, 더 강력하고 효과 좋은 마법진과 희귀한 재화를 얻기 위해 헥사곤 던전을 돌파하는 경영 & 탐험 게임입니다.

### 💡 주요 특징 (Key Highlights)
1. **제스처 기반 마법 드로잉 시스템 (Gesture Drawing System):**
   플레이어가 입력한 좌표 데이터를 실시간으로 샘플링하여, 사전 정의된 템플릿(원, 별, 번개 등)과 유사도를 점수화(Scoring)해 마법을 시전하는 시스템을 구현했습니다.
2. **공간 분석 기반 콤보 매칭 알고리즘 (Spatial Rule Combo Matching):**
   여러 도형을 그렸을 때 단순히 도형의 종류만 확인하는 것을 넘어 도형 간의 공간적 관계(Inside, Surround 등)와 중복 방지(Global Overlap) 규칙을 수학적으로 분석하여 고위력의 콤보 마법을 발동시킵니다.
3. **AP(Action Point) 기반 헥사곤 탐색 시스템:**
   Cost 기반의 헥스 패스파인딩(Hex Pathfinding)을 적용해 AP 안에서 이동 가능한 범위를 시각화하고, 타일 미리보기(Peek) 기능을 통해 턴제 던전 탐험의 전략성을 극대화했습니다.
4. **AI 기반 아트워크 및 룩앤필 구성 (AI-Driven Assets):**
   직접 프롬프트를 연구하고 디자인 가이드를 설정하여 생성형 AI로 게임의 핵심 에셋을 추출 및 가공하여 1인 개발임에도 높은 수준의 그래픽 퀄리티를 달성했습니다.

---

## 🛠 Tech Stack

### **핵심 라이브러리 및 기술**
- **DOTween:** 부드러운 UI 트랜지션 및 인게임 애니메이션 제어
- **Unity 2D Tilemap:** 헥사곤(Hexagon) 기반 맵 제네레이션 및 타일 시스템
- **PointCloud / $1 Gesture Recognition:** 사용자 드로잉 스트로크(Stroke) 패턴 인식 및 분석 알고리즘

---

## 🔥 Challenge & Solution

### 부동소수점 오차로 인한 무한 루프 및 에디터 프리징 해결
**Problem:** 
사용자가 입력한 자유로운 선(Stroke)의 좌표들을 인식하기 위해 64개의 균일한 간격의 점으로 리샘플링(Resampling)하는 로직(`PointCloudRecognizer`)을 구현했습니다. 하지만, 점과 점 사이의 거리(`d`)가 극단적으로 짧은 상태에서 마우스가 머물러 있을 때 부동소수점 오차로 인해 `d`가 0에 한없이 수렴하게 되었습니다. 이로 인해 나눗셈 연산에서 유효하지 않은 값(NaN)이 발생하고, 목표 점의 개수를 채우지 못해 while 루프를 빠져나오지 못하면서 유니티 에디터가 완전히 프리징되는 치명적인 버그가 발생했습니다.

**Solution:** 
AI(Antigravity)와의 코드 분석을 통해 루프 내부의 논리적 결함을 특정했습니다. 
1. 거리 `d`가 `0.0001f` 이하일 경우 부동소수점 오류 방지를 위해 나눗셈 기반의 보간 연산을 건너뛰도록 하드 리미트를 설정했습니다. 
2. 리샘플링된 점들의 리스트(`newPoints`)가 목표 개수(`n`)에 도달하면 즉시 break로 루프를 강제 종료하는 명시적인 Failsafe 방어 로직(`if (newPoints.Count >= n) break;`)을 최상단에 추가했습니다. 
결과적으로 어떠한 악의적이거나 극단적인 입력이 들어오더라도 O(N)의 시간 복잡도 내에서 안전하게 리샘플링을 마치도록 보장하여, 치명적인 프리징 이슈를 완벽하게 해결했습니다.
