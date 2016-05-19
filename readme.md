Namu
====
![mock](logo.png)<br>
__(Namu-Mock)__

목업 설정하기
----
__Static Method__
```c#
Mock.Method(() => MyClass.Method())
  .Should(10);
```

__Method__
```c#
Mock.Select<MyClass>()
  .Method(x => x.Method())
  .Should(10);
```

__Property__
```c#
Mock.Select<MyClass>()
  .Method(x => x.Property)
  .Should(10);
```

__Runtime Mock__<br>
런타임에 실행되는 값 생성기를 설정하여 런타임 목업을 설정할 수 있습니다.<br>
생성기는 매번 실행됩니다.
```c#
Mock.Select<MyClass>()
  .Method(x => x.JINWOOTIME)
  .Should(() => DateTime.Now.ToString());
```

목업된 값 가져오기
----
목업에 의해 `null`이 설정되는 케이스도 존재하므로, 설정된 값을 가져오는 작업은 아래와 같이 2 단계를 거칩니다.
```c#
int SomeMethod() {
  if (Mock.IsRegistered) return (int)Mock.Value;
}
```
`Namu`의 API들은 자동으로 caller 정보를 가져오기 때문에 호출자의 정보를 같이 넘겨줄 필요가 없습니다.

스레드 안전성
----
기본적으로 `Namu`의 모든 public API들은 스레드에 안전합니다.<br>
하지만 멀티스레드 환경에서는 여전히 아래와 같은 시나리오가 발생할 수 있기 때문에, `Namu`는 트랜젝션 API를 제공합니다.
```
IsRegistered -> True 로 인해서 IF 진입
Mock.Unregister 
Value 실행 시 값이 없으므로 익셉션 발생
```
아래의 예제들은 트랜젝션을 사용하여 전체 동작을 스레드에 안전하게 만드는 방법을 보여줍니다.
```c#
int SomeMethod() {
  if (Mock.IsRegisteredTx) return Mock.Value;
}
```
`IsRegisteredTx` API가 호출되면 트랜젝션이 시작됩니다. 다음번 `Value`가 실행될 때 까지 크리티컬 섹션에 진입한 상태로 남아있게 됨에 주의해 주세요.

```c#
int SomeMethod() {
  int value;
  if (Mock.TryGetValue(out value)) return value;
}
```
또는 위와 같이 단일 API 호출로 대체할 수 있습니다.
