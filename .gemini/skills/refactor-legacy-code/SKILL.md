---
name: refactor-legacy-code
description: |
  リファクタリング: テストのないレガシーコードを、安全性を担保するために再構築する。
  Gitによるステータス確認および挙動を固定する仕様化テスト（ゴールデンマスターパターン）を使用。
  テストコードが存在しないプロジェクトでのコード整理、バグ修正、または機能改修を依頼された場合に使用。
---

Strictly follow this **Legacy Code Refactoring Protocol**. Avoid "Edit
and Pray" at all costs.

## Phase 1: Assess & Secure (Safety First)

1. **Check Environment**: Run `ls -a` to check for `.git` and dependency files.
2. **Mandatory Save Point**:
   - IF `.git` exists: Check `git status`. IF dirty, instruct user to
commit/stash.
   - IF NO `.git`: Instruct user to initialize git.
   - **Constraint**: Do NOT proceed without a clean state.

## Phase 2: Lock Down (Characterization Tests)

**GOAL**: Capture current behavior (including bugs) mechanically.

1. **No Logic Changes**: Do not touch production code yet.
2. **Golden Master Pattern**: Create a system/snapshot test that
records the output of the main handler/function.
   - _Strategy_: Treat code as a black box. Feed inputs, record exact outputs.
   - _Success Criteria_: Any future change in behavior must fail this test.

## Phase 3: Divide & Conquer (Refactor)

Only proceed after Phase 2 tests PASS.

1. **Separate I/O & Logic**: Extract business logic into **Pure
Functions**. Leave I/O side effects in the thin outer layer.
2. **Rename**: Update variables/functions to reflect intent.
3. **Handling Bugs**:
   - IF a bug is found: Do **NOT** fix it yet.
   - Action: Document it as a test case to ensure reproduction.
   - Fix bugs only after refactoring is complete.

## Output Rule

Start your first response by verifying the safety state:
"Refactoring Protocol Initiated. Checking Git status and safety measures..."
