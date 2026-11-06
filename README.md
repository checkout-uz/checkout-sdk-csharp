# CheckoutUz.Sdk

[CHECKOUT.UZ](https://checkout.uz) merchant API'si uchun rasmiy .NET kutubxonasi — bitta paket orqali **v1** va **v2** versiyalarining ikkalasiga ham kirish imkoni.

```csharp
var client = new CheckoutUz.CheckoutClient("kassangizning_api_kaliti");
var payment = await client.V2.CreatePaymentAsync(50000, description: "Buyurtma #1");
Console.WriteLine(payment["payment"]!["_url"]); // shu havolani mijozga yuboring
```

## Talablar

- **.NET 6.0+** (`System.Text.Json`'dan foydalanadi, tashqi HTTP/JSON kutubxonasi shart emas)

## O'rnatish

Hozircha NuGet'da emas. Manba kodni to'g'ridan-to'g'ri loyihangizga qo'shing:

```bash
git clone https://github.com/checkout-uz/checkout-sdk-csharp.git
```

va `.csproj`'ingizga loyiha havolasini qo'shing:

```xml
<ItemGroup>
    <ProjectReference Include="../checkout-sdk-csharp/src/CheckoutUz/CheckoutUz.csproj" />
</ItemGroup>
```

## API kalitini qayerdan olish mumkin

1. [checkout.uz](https://checkout.uz)ga kiring → **Kassalar** → kerakli kassani oching → **Sozlamalar**.
2. U yerda ko'rsatilgan **API KEY**ni nusxalang.
3. Bu kalitni hech kimga bermang — faqat server tomonida (backend) ishlating.

## Tezkor boshlash

```csharp
using CheckoutUz;

var client = new CheckoutClient("kassangizning_api_kaliti");

try
{
    var payment = await client.V2.CreatePaymentAsync(
        amount: 50000,                 // so'mda, 1000 - 10 000 000 oralig'ida
        description: "Buyurtma #1",
        expireTime: 3600               // ixtiyoriy, soniyada, standart 1 soat
    );
    var p = payment["payment"]!;
    Console.WriteLine(p["_url"]!.GetValue<string>());   // mijozni shu to'lov sahifasiga yo'naltiring
    Console.WriteLine(p["_uuid"]!.GetValue<string>());  // buni saqlab qo'ying - to'lovning doimiy ID'si
}
catch (CheckoutApiException e)
{
    Console.Error.WriteLine($"Xatolik ({e.StatusCode}): {e.Message}");
}
```

Mijoz to'lov qilgandan keyin, checkout.uz sizning `webhookUrl`'ingizga xabar yuboradi — yoki `StatusPaymentAsync` orqali o'zingiz tekshirishingiz mumkin:

```csharp
var status = await client.V2.StatusPaymentAsync(payment["payment"]!["_uuid"]!.GetValue<string>());
Console.WriteLine(status["data"]!["status"]!.GetValue<string>()); // "pending" / "paid" / "canceled" / "reversed"
```

## v1 va v2 farqi

```csharp
await client.V1.CreatePaymentAsync(50000);
await client.V2.CreatePaymentAsync(50000, expireTime: 7200, idempotencyKey: "order-42");
```

v2 v1'ning hamma imkoniyatlari ustiga quyidagilarni qo'shadi:

- `idempotencyKey` (`CreatePaymentAsync`da) — xavfsiz qayta urinish
- `expireTime` — invoys qancha vaqt ochiq turishini belgilash
- `GetHistoryAsync(offset:, status:, dateFrom:, dateTo:)` — sahifalash va filtrlash
- `GetStatsAsync()` — endi `by_provider` taqsimot bilan
- `CancelPaymentAsync(orderId)` — hali "kutilmoqda" holatidagi invoysni bekor qilish
- `RefundPaymentAsync(orderId)` — muvaffaqiyatli to'lovni qaytarish
- `GetFiscalAsync(orderId)` — fiskal chekni olish

## Hisob darajasidagi metodlar (`ProfileClient`)

```csharp
var profile = new ProfileClient("profil_api_kalitingiz");
var shops = await profile.GetShopsAsync();
var stats = await profile.GetProfileStatsAsync(dateFrom: "2026-01-01");
```

## Xatoliklarni qayta ishlash

Har bir metod muvaffaqiyatsizlikda `CheckoutApiException` chiqaradi:

```csharp
try
{
    await client.V2.RefundPaymentAsync(123);
}
catch (CheckoutApiException e)
{
    Console.WriteLine($"{e.StatusCode} {e.Message}");
}
```

## To'liq API hujjatlari

- v1: https://checkout.uz/api-docs/v1
- v2: https://checkout.uz/api-docs/v2

## Ishlab chiqish (development)

```bash
dotnet test
```

Barcha testlar HTTP transportini soxtalashtiradi (`FakeTransport`) — hech biri haqiqiy checkout.uz API'siga murojaat qilmaydi.

## Litsenziya

MIT
