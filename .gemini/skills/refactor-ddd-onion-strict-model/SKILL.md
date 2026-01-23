---
name: refactor-strict-ddd-onion-complete
description: |
    設計: ソースコードをオニオンアーキテクチャへの準拠とドメイン知識の凝集度向上のために処理する。
    Value Objectの抽出、Entityへのロジック集約、1メソッドUseCase化、依存性逆転（DIP）を使用。
    ドメインモデル貧血症、ドメインサービスの散在、またはレイヤー境界の曖昧化が検知された時に使用。
version: 1.3.0
---

# Refactor Strict DDD Onion

**Context:** オニオンアーキテクチャを採用し、ドメインサービスを排除してモデルに知識を完全集約するプロジェクトに適用する。

## Rules / Actions / Steps / Solution

### 1. Value Object Extraction (値オブジェクトへの知識割当)

* **Trigger**: プリミティブ型が使われている箇所で、バリデーション、比較、特定のフォーマット変換ロジックがUseCaseや外部に漏れている場合。
* **Action**:
1. 専用の **Value Object (VO)** クラスを作成し、不変（Immutable）にする。
2. バリデーションをコンストラクタに、計算・加工ロジックをメソッドに閉じ込める。
3. **ドメインサービスを作る代わりに、可能な限り計算ロジックをVOのメソッド（またはstaticメソッド）として定義する。**


### 2. Absolute Domain Logic Consolidation (ドメインサービス禁止とEntity集約)

* **Trigger**: ビジネスルール（if文や計算）が `*Service` クラスに記述されている、またはEntityがデータ保持のみを行っている場合。
* **Action**:
1. **ドメインサービスクラスの作成・利用を原則禁止とする。**
2. Serviceにあるロジックを、その知識を所有すべき **Entity** または **VO** へ移動する。
3. 複数オブジェクトが関わる場合も、主導するEntityが他のEntity/VOを引数で受け取り、自身の状態を更新する形式にする。


### 3. Single-Method UseCase Class (1クラス1メソッドUseCase)

* **Trigger**: Application層で1クラスに複数のパブリックメソッドがある、または `*Service` という命名がされている場合。
* **Action**:
1. 各ユースケースを `CancelOrderUseCase` のように独立したクラスへ分割する。
2. パブリックメソッドは `execute()` 1つのみとする。
3. 依存先ははドメイン層（Entity, VO, Repositoryのインターフェース）のみとする。 Infrastructure層（DB実装、外部APIクライアントの実装）には決して依存せず、Domain層で定義したインターフェースを介する。


### 4. Onion Architecture & DIP (オニオン境界の防御)

* **Trigger**: Domain層が外部ライブラリ（ORM、Webフレームワーク）や他レイヤーに依存している場合。
* **Action**:
1. インフラ依存（DB、API、外部通知）の抽象（Interface）をDomain層に定義する。
2. 実装はInfrastructure層に記述し、実行時に注入する。依存の矢印が常に内側（Domain）を向くように制御する。


### 5. Aggregate Boundary Protection (集約による整合性保護)

* **Trigger**: UseCaseから子Entityを直接操作している、または複数の集約を1トランザクションで同時に更新しようとしている場合。
* **Action**:
1. 集約ルート以外への直接アクセスを禁止し、全ての操作をルート経由にする。
2. 他の集約を参照する場合はオブジェクトではなく「ID」で保持する。


## Example

**Before: 手続き的で技術依存が強い「貧血症」なService**

```typescript
// app/services/UserService.ts
// 諸悪の根源: 複数責務、インフラ（DB）直接依存、ドメインロジックの漏出
class UserService {
  constructor(private db: any) {}

  async register(data: any) {
    // プリミティブへの依存とバリデーションの漏出
    if (!data.email.includes("@")) throw new Error("Invalid email");

    // ドメインロジック（重複チェックやハッシュ化）がサービスにある
    const existing = await this.db.table("users").where("email", data.email).first();
    if (existing) throw new Error("Already exists");

    const user = { ...data, status: "PENDING", createdAt: new Date() };
    await this.db.table("users").insert(user);
    // メール送信（インフラ）が直接書かれている
    await axios.post("https://api.mail.com/send", { to: data.email });
  }

  async upgradePlan(userId: string, plan: string) { /* 別のロジックが混在... */ }
}

```

**After: 厳格なオニオンアーキテクチャと凝集されたドメインモデル**

```typescript
// --- Domain Layer ---
// 1. Value Object: 自己バリデーションと不変性の保証
class Email {
  constructor(private readonly value: string) {
    if (!value.includes("@")) throw new Error("Invalid email");
  }
  toString() { return this.value; }
}

// 2. Entity: 振る舞いと状態遷移の集約（Domain Serviceは作らない）
class User {
  private status: UserStatus = UserStatus.Pending;
  constructor(private readonly id: UserId, private readonly email: Email) {}

  // 状態変更のビジネスルールをEntity内に封じ込める
  activate() {
    this.status = UserStatus.Active;
  }
}

// 3. Repository Interface: インフラを抽象化
interface IUserRepository {
  nextId(): UserId;
  findByEmail(email: Email): Promise<User | null>;
  save(user: User): Promise<void>;
}

// --- Application Layer ---
// 4. Single-Method UseCase: オーケストレーションに専念
class RegisterUserUseCase {
  constructor(
    private readonly userRepo: IUserRepository, // Domain層のInterfaceに依存
    private readonly mailer: IMailer          // 外部サービスもInterface経由
  ) {}

  async execute(input: RegisterInput): Promise<void> {
    const email = new Email(input.email);

    // ドメインルール（一意性等）のチェックもUseCaseはRepositoryに聞くだけ
    const existing = await this.userRepo.findByEmail(email);
    if (existing) throw new AlreadyExistsException();

    const user = new User(this.userRepo.nextId(), email);
    await this.userRepo.save(user);
    await this.mailer.sendWelcomeMail(email);
  }
}

```

## When to Use

* `DomainService` が肥大化し、ビジネスルールがどこにあるか見失ったとき。
* 値の妥当性チェックがシステムの至る所に散乱しているとき。
* オニオンアーキテクチャのレイヤー境界を自動テストや静的解析で守りたいとき。
