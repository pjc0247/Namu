Namu
====
![mock](logo.png)<br>
__(Namu-Mock)__

��� �����ϱ�
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
��Ÿ�ӿ� ����Ǵ� �� �����⸦ �����Ͽ� ��Ÿ�� ����� ������ �� �ֽ��ϴ�.<br>
������� �Ź� ����˴ϴ�.
```c#
Mock.Select<MyClass>()
  .Method(x => x.JINWOOTIME)
  .Should(() => DateTime.Now.ToString());
```

����� �� ��������
----
����� ���� `null`�� �����Ǵ� ���̽��� �����ϹǷ�, ������ ���� �������� �۾��� �Ʒ��� ���� 2 �ܰ踦 ��Ĩ�ϴ�.
```c#
int SomeMethod() {
  if (Mock.IsRegistered) return (int)Mock.Value;
}
```
`Namu`�� API���� �ڵ����� caller ������ �������� ������ ȣ������ ������ ���� �Ѱ��� �ʿ䰡 �����ϴ�.

������ ������
----
�⺻������ `Namu`�� ��� public API���� �����忡 �����մϴ�.<br>
������ ��Ƽ������ ȯ�濡���� ������ �Ʒ��� ���� �ó������� �߻��� �� �ֱ� ������, `Namu`�� Ʈ������ API�� �����մϴ�.
```
IsRegistered -> True �� ���ؼ� IF ����
Mock.Unregister 
Value ���� �� ���� �����Ƿ� �ͼ��� �߻�
```
�Ʒ��� �������� Ʈ�������� ����Ͽ� ��ü ������ �����忡 �����ϰ� ����� ����� �����ݴϴ�.
```c#
int SomeMethod() {
  if (Mock.IsRegisteredTx) return Mock.Value;
}
```
`IsRegisteredTx` API�� ȣ��Ǹ� Ʈ�������� ���۵˴ϴ�. ������ `Value`�� ����� �� ���� ũ��Ƽ�� ���ǿ� ������ ���·� �����ְ� �ʿ� ������ �ּ���.

```c#
int SomeMethod() {
  int value;
  if (Mock.TryGetValue(out value)) return value;
}
```
�Ǵ� ���� ���� ���� API ȣ��� ��ü�� �� �ֽ��ϴ�.