# İki Oyunculu Oyun Kurulum Rehberi

Bu rehber, oyunu iki oyunculu split-screen modu ile kurmanız için adımları açıklar.

## 1. GamepadInput Bileşeni Ekle

Mevcut oyuncu kartına (playerKart) GamepadInput bileşeni eklendi. İkinci oyuncu kartı için de yapmanız gerekir:

### Oyuncu 1 (İlk Kart):
1. Hierarchy'de ilk kart objesini seçin
2. Inspector'da **Add Component** tıklayın
3. `GamepadInput` arayın ve seçin
4. Ayarlar:
   - **Joystick Number**: 0 (İlk Gamepad)
   - Bu kart Oyuncu 1 için kullanılacak

### Oyuncu 2 (İkinci Kart):
1. Oyuncu 1 kartını Ctrl+D ile çoğaltın (veya yeni bir kart ekleyin)
2. Yeni kartı farklı bir başlangıç konumuna yerleştirin
3. Yeni kart objesine `GamepadInput` bileşeni ekleyin
4. Ayarlar:
   - **Joystick Number**: 1 (İkinci Gamepad)

## 2. Oyuncu 2 Kontrol Seçeneği

İkinci oyuncu için iki seçeneğiniz var:

### Seçenek A: Gamepad (Her iki oyuncu Gamepad)
Oyuncu 2 kartına `GamepadInput` bileşeni ekleyin:
1. Oyuncu 2 kartını seçin
2. **Add Component** tıklayın
3. `GamepadInput` arayın ve ekleyin
4. **Joystick Number**: 1 (İkinci Gamepad)

### Seçenek B: Klavye (WASD - Tavsiye Edilen Gamepad Yokken!)
Oyuncu 2 kartına `AlternateKeyboardInput` bileşeni ekleyin:
1. Oyuncu 2 kartını seçin
2. **Add Component** tıklayın
3. `AlternateKeyboardInput` arayın ve ekleyin
4. Kontroller:
   - **W**: İleri
   - **S**: Geri
   - **A**: Sol
   - **D**: Sağ
   - **Space**: Turbo

**TwoPlayerSetupHelper Kullanırken:**
- Inspector'da `Use Keyboard For Player2` checkbox'ını işaretleyin
- Helper otomatik olarak AlternateKeyboardInput ekleyecek
1. Hierarchy'de boş bir GameObject oluşturun: `GameObject > Create Empty`
2. Adını `SplitScreenCameraManager` yapın
3. `Add Component` tıklayın ve `SplitScreenCameraManager` arayın
4. Bileşeni ekleyin:
   - **Player 1 Camera**: Ana kamera (Canvas'ı süpüren dikey)
   - **Player 2 Camera**: İkinci oyuncu için Cinemachine sanal kamerası

### Cinemachine Sanal Kameralarını Ayarlayın:
1. Oyuncu 1 kartında **ArcadeKart** bileşenini bulun
   - **vCam** alanına, Oyuncu 1'in `CinemachineVirtualCamera`'sı atanmış olmalı
   
2. Oyuncu 2 kartında aynı şeyi yapın (farklı bir Cinemachine kamerası)

## 3. GameFlowManager Güncellemeleri

Hierarchy'de `GameFlowManager` objesini seçin ve Inspector'da:

1. **Two Player Mode**: Etkinleştirin
   - Checkbox'ı işaretleyin
2. **Auto Find Karts**: Açık bırakın
3. **Player Kart**: İlk kartı atayın
4. **Player2 Kart**: İkinci kartı atayın

## 4. Kayıt Alanları (InputSystem)

Oyunda şu kayıt alanları kullanılıyor olmalıdır:
- `Accelerate`: Hızlanmak için
- `Brake`: Frenleme için
- `Horizontal`: Dönüş için
- `Nitro`: Turbo artırma için

Her Gamepad için Unity'nin Input Manager'ında (Edit > Project Settings > Input Manager):
- Joystick1/Joystick2 ön ekleri ile derlenmiş kayıtlar varsa ideal
- Yoksa custom binding'ler oluşturmanız gerekebilir

## 5. Test Etme

1. Oyunun başında `Canvas > Start Canvas`'dan "Start" tıklayın
2. Her iki Gamepad de bağlı olmalı:
   - Gamepad 1: Oyuncu 1
   - Gamepad 2: Oyuncu 2
3. Oyun başlıyor, her oyuncu kendi kartını kontrol edebilir
4. Sol taraf: Oyuncu 1
5. Sağ taraf: Oyuncu 2

## Gamepad Kontrolleri

### Varsayılan Tuşlar:
- **İleri**: RT (Right Trigger) - Hızlanma
- **Geri**: LT (Left Trigger) - Frenleme
- **Dönüş**: Sol Analog Stick (Sol/Sağ)
- **Turbo (Nitro)**: X veya Y Butonu (oyun ayarlarına bağlı)

## Sorunda Mı?

### Gamepads Algılanmıyor:
- Windows > Unity > Help > About > Joystick Status'u kontrol edin
- Cihazları yeniden bağlamayı deneyin

### Split-Screen Görünmüyor:
- Main Camera component'inin rect değerini kontrol edin (0, 0, 0.5, 1)
- İkinci kamera'nın rect değeri (0.5, 0, 0.5, 1) olmalı

### Kart Yanlış Kontrol Açısından Hareket Ediyor:
- ArcadeKart bileşeninin **vCam** alanı düzgün atandığından emin olun
- Cinemachine kameralarının Follow ve LookAt değerleri kontrol edin

## Yazı Türü Değişiklikleri (Kod Tarafı)

Eklenen yeni classlar:
- `GamepadInput.cs`: Gamepad girdisi için yeni input sınıfı
- `SplitScreenCameraManager.cs`: Split-screen kamera yönetimi
- `GameFlowManager.cs`: İki oyuncu desteği için güncellenmiş

Tüm bu değişiklikler, mevcut single-player sistemi bozmaksızın çalışır.
