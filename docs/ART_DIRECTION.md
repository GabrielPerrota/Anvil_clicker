# Anvil Clicker — Direção de Arte

> **Dono:** diretor de arte (agente `art-director`). **Versão:** 1.0 (2026-10-06).
> Este guia vale para toda arte do jogo: sprites, tiles, luz, efeitos e UI. A arte é **pixel art gerada por código** no Editor da Unity: determinística, idempotente, sem assets de terceiros.
> Este documento assume o redesenho do jogo em torno de **missões, encomendas de clientes e uma economia pequena** (poucos aprendizes com nome, números modestos). A arte não deve sugerir "fábrica de armas"; deve sugerir **uma oficina de bairro com clientes de verdade**.

---

## 1. Visão e pilares visuais

**Em uma frase:** *uma ferraria pequena e aconchegante, onde cada objeto tem história, e que fica mais cheia, mais clara e mais bonita a cada encomenda entregue.*

| # | Pilar | O que significa na tela | Como se testa |
|---|---|---|---|
| 1 | **A forja é o coração** | Única luz quente forte da cena; tudo ao redor é mais frio e escuro. O olho vai para a brasa primeiro, depois para o ferreiro. | Em uma captura reduzida a 25% e em tons de cinza, o ponto mais claro é a forja/bigorna. |
| 2 | **Silhueta antes de detalhe** | Ferreiro, clientes e estações são reconhecíveis só pelo contorno preenchido de preto. Nenhum móvel é "um cubo com cor diferente". | Teste da silhueta: pintar tudo de preto e ainda identificar cada objeto. |
| 3 | **Pequeno e denso** | Poucos objetos, cada um com peso: ferramentas penduradas, barris, caixotes, vigas. Nada de chão vazio maior que 2 × 2 células sem prop, tapete ou sombra. | Nenhuma área de piso nua maior que 2 × 2 células na Oficina. |
| 4 | **Prosperidade visível** | A loja melhora em 3 estágios (Humilde, Estabelecida, Próspera): mais luz, mais objetos, materiais melhores. O jogador *vê* as missões cumpridas no cenário. | Capturas lado a lado dos 3 estágios são distinguíveis em 2 s. |
| 5 | **Artesanal, não neon** | Madeira, pedra, couro, ferro e brasa. Saturação baixa, exceto a brasa e o ouro (únicos "acentos vivos"). | Nenhuma cor da paleta fora da lista da seção 4 em uso. |

**Referências de sensação** (sem copiar arte): oficina de vila em RPG de 16 bits; luz de lareira em taverna; materiais "usados" (marcas de queimado, ferrugem, couro gasto).

---

## 2. Auditoria do estado atual

Baseada nas capturas `m3-01-start`, `m3-04-striking`, `m3-07-desk-open`, `m3-13-walk-to-storage` e nos sprites em `Assets/_Project/Art/Placeholders/`.

### 2.1 O que funciona (manter)

- **A forja brilha.** O topo laranja da forja e as duas "bocas" de fogo são a coisa mais legível da cena e já criam o ponto focal (`ForgeDetails`). A luz 2D pontual (`Light2D`, raio 3,2) tinge o chão ao redor de forma convincente.
- **Geometria isométrica correta.** Tile 64 × 32, blocos 64 × (32 + h), pivô no centro da pegada, ordenação por Y: a cena não "quebra" ao andar.
- **Contorno escuro** (`#18141A`) em blocos e personagem dá coesão.
- **Faíscas e números flutuantes** dão peso ao golpe (`m3-04`).
- **Cutaway:** só as paredes de trás são desenhadas, o que deixa ver o interior. Manter.

### 2.2 O que está fraco (corrigir)

| Área | Problema (específico) | Efeito | Correção (seção) |
|---|---|---|---|
| **Silhueta do ferreiro** | Sprite 32 × 48 com corpo de ~20 × 42 px. Sem barba, sem lenço, sem avental legível (retângulo marrom de 10 × 12 px), martelo = 3 × 10 px cinza + cabeça 8 × 5 px quase invisível. Cabelo e avental marrons são quase a cor do piso e da madeira. | Lê-se como "aldeão genérico". Perde-se no piso (`m3-01`, `m3-13`). | §5 |
| **Contraste** | Piso `#463A34` e calça `#343040` são ambos escuros e frios/neutros; avental `#604630` ≈ madeira dos caixotes `#7C583A`. Só a túnica vermelha se destaca. | O ferreiro "some" da cintura para baixo. | §5.3, regra de luminosidade §3.7 |
| **Escala relativa** | Bigorna 64 × 48 px (1 unidade de largura) é **2,5× mais larga que o ferreiro** (~24 px). Caixotes/mesas têm 20–26 px de altura, abaixo da cintura do ferreiro; a parede (34 px) mal passa da cabeça dele. | Sensação de maquete, não de oficina. O ferreiro parece uma criança diante de uma bigorna gigante. | §3.4 (alturas), §7 |
| **Piso** | Um único tile de losango marrom com ruído aleatório (`Dither`, amplitude 10) e contorno escuro grosso; os losangos formam uma grade de "colmeia" repetitiva. | Fica "sujo" em vez de ser tábua/pedra; a grade de contornos domina a cena. | §6.2 |
| **Paredes** | Bloco cinza de 34 px com tijolos de 8 px e ruído; a face de cima e as laterais têm a mesma cor em todas as salas. Sem janela, sem viga, sem reboco, sem porta. | Parece um muro de masmorra, não uma loja. Zero personalidade. | §6.1 |
| **Props** | Mesa, balcão, mensageiro e depósito são **o mesmo cubo** (`BlockStyle`) com tampo de cor diferente e um detalhe de 4–14 px. Silhuetas idênticas. | Impossível distinguir sem ler a cor. Falha no pilar 2. | §7 |
| **Ruído de pixel** | `Dither()` aplica ±5 a ±10 de brilho **aleatório por pixel** em todo lugar. | Efeito "sal e pimenta" que não é pixel art de verdade: tudo parece granulado, sem material definido. | §3.6: dithering ordenado, não aleatório |
| **Iluminação** | Só a forja e a bigorna emitem luz. O resto cai quase a preto (`#100E12` fora do losango). Sem janela, sem luz ambiente fria legível. | Cena "mergulhada num poço"; os cantos somem. | §3.8 |
| **Aprendizes** | `worker.png`: 24 × 32, boneco branco ou azul, idêntico, sem rosto. | Nenhum vínculo emocional; contradiz "aprendizes com nome". | §8.2 |
| **Pixel perfeito** | A câmera exibe a arte em zoom fracionário (~1,56×): bordas "serrilhadas" irregulares em `m3-04` (pixels de tamanhos diferentes). | Pixel art parece borrada/irregular. | §3.2 (zoom inteiro) e pergunta aberta 1 |
| **UI** | Painéis translúcidos com borda fina marrom e cantos arredondados; moeda é círculo chapado; fonte padrão sem identidade; legenda "ouro" de ~11 px; dicas de ~12–13 px com 70% de opacidade; números flutuantes `+1` brancos quase invisíveis sobre a parede clara (`m3-04`). | Parece um app web genérico por cima do jogo. | §10 |
| **Salas** | O Depósito (`m3-13`) é um quadrado de piso com 1 caixote: sem parede lateral completa, sem porta, sem prateleira. | Não parece "um cômodo de verdade". | §6.3 |

**Nota:** a auditoria é sobre arte *placeholder*, que cumpriu o papel de provar a pipeline. Nada disso exige mudar a lógica; só os geradores do Editor.

---

## 3. Regras técnicas do mundo isométrico

### 3.1 Grade e escala

| Item | Valor | Observação |
|---|---|---|
| Projeção | Isométrica 2:1 (dimétrica) | ângulo de 26,57° |
| Célula de mundo | **1 × 0,5 unidade** | Grid do Unity: cell size (1, 0,5, 1), layout Isometric |
| Pixels por unidade (PPU) | **64** para *todo* sprite do mundo | O `anvil.png` atual usa 32 PPU e escala 0,5 na prefab: **migrar para 64 PPU** (arte redesenhada) |
| Tile de piso | **64 × 32 px**, losango | pivô (0,5, 0,5) |
| Bloco / parede | **64 × (32 + h) px** | pivô `(0,5, 16 / altura)`, ou seja, no centro da pegada (losango inferior) |
| Personagem | pivô **nos pés**: `(0,5, 4 / altura_da_tela)` | Os 4 px abaixo dos pés guardam a sombra de contato |
| Textura | `FilterMode.Point`, sem compressão, sem mipmap, `SpriteMeshType.FullRect` | já aplicado em `PlaceholderArtGenerator.WriteSprite` |
| Pixel real | 1 px de textura = 1 px de arte. Proibido sub-pixel, rotação ou escala fracionária de sprites. | Escalas só 1×; zoom é da câmera |

### 3.2 Zoom e pixel perfeito
- A câmera deve usar **zoom inteiro** (2× ou 3× do PPU de referência 64). Em 1280 × 720, **2×** mostra uma Oficina 8 × 8 inteira (512 px de largura ×2 = 1024 px) com paredes de 72 px (altura total ~330 px ×2 ≈ 660 px). **Modo forja:** 3×.
- Se a câmera precisar de zoom contínuo (scroll), ao soltar o scroll ela "encaixa" no inteiro mais próximo. (Mudança de Presentation: pedir ao agente principal; ver pergunta aberta 1.)

### 3.3 Ordenação
- Camada de ordenação `World`: ordem = `-Y` do pé do objeto (pivô). Estação e personagem usam o mesmo critério. Props de parede (quadros, ferramentas penduradas) pertencem à parede e herdam sua ordem.
- Chão sempre abaixo (`Floor`), luz/FX acima (`FX`), UI fora do mundo.
- Objetos altos (forja com chaminé, prateleira) ficam **atrás** do personagem quando o pé do personagem tem Y menor. Usar pegada pequena (losango 0,9 × 0,45) para o jogador poder passar rente sem "entrar" no objeto.

### 3.4 Alturas de referência (px acima do plano do chão)

| Referência | Altura | Uso |
|---|---|---|
| Ferreiro (pé até o topo do lenço) | **48** | régua de tudo |
| Joelho | 12 | pedra de assento, degrau |
| Cintura / bancada | 22–26 | mesa, balcão, bigorna (topo) |
| Ombro | 36 | caixote empilhado, tonel |
| Cabeça | 48 | prateleira baixa |
| Parede de fundo | **72** (antes 34) | paredes NE/NW; viga a 66; janela de 22 a 52 |
| Chaminé / viga alta | 100 | forja |

Por que 48 px e não 56–64: com cabeça de 16 px (1/3 do corpo, proporção "chibi" de RPG) o rosto com barba cabe em 16 × 16 px legíveis, e 48 px já ocupa 1,5 células de altura (32 px por célula), suficiente para um ícone de "pessoa". 56–64 px exigiria paredes de 100+ px e estouraria os 720 px de altura da Oficina a 2×. Se o usuário preferir um personagem maior, escala-se tudo ×1,17 (ver pergunta aberta).

### 3.5 Contornos
- Contorno de **1 px**, cor `#1B1418` (Tinta), em personagens e props. **Contorno seletivo:** na borda voltada para a luz usar um tom 1 passo mais claro do material em vez de tinta (ex.: topo do avental), para o objeto "descolar" do fundo.
- Dentro do sprite, separação de formas por mudança de cor, não por contorno preto (só nos limites de materiais muito diferentes, como luva × cabo).
- Tiles de piso: **sem contorno** externo; as emendas são linhas de tom 1 passo mais escuro (§6.2). Hoje o losango tem contorno grosso, o que cria a "colmeia".

### 3.6 Sombreamento e textura
- **3 tons por material** (luz, base, sombra) + realce opcional. Luz vem de cima-esquerda (convenção do jogo) para sprites sem luz dinâmica: face esquerda do bloco = base, face direita = sombra, topo = luz.
- **Sem ruído aleatório por pixel.** Substituir `Dither()` por: (a) **dithering ordenado 2 × 2** (padrão xadrez) só em transições de tom, e (b) **manchas desenhadas à mão** (listas de pixels) para sujeira, ferrugem e queimado. Determinístico por construção.
- Textura de material: pedra = blocos irregulares com junta escura e 1 rachadura; madeira = veios horizontais de 1 px com 1 nó; reboco = base lisa com 1–2 manchas e canto descascado; ferro = faixa de brilho de 1 px no topo.

### 3.7 Contraste e leitura
- **Regra de luminosidade:** piso e paredes ficam na faixa de luminosidade 12–38%; personagens e objetos interativos com ao menos uma região de luminosidade ≥ 55% (pele, lenço, punho, brasa, ouro). 
- **Regra de cor de acento:** o ferreiro é o **único** a usar a "ferrugem-vermelha" `#B4472E` na cena (clientes usam outras cores de facção).
- **Rim light:** personagens têm 1 px de realce quente (`#F2B680`) no lado voltado para a forja (aplicado por código quando se gera a variante; ou pelo `Light2D` normal).
- Clique/interação: estação ao alcance recebe **contorno claro de 1 px** (`#FFE08A`) pulsando, mais o prompt "E — Nome".

### 3.8 Luz e ambiente (URP 2D)

| Luz | Cor | Intensidade | Raio / alcance | Notas |
|---|---|---|---|---|
| Ambiente global | `#2B3350` (azul-noite frio) | **0,45** (hoje ~0,2: cantos somem) | global | sobe 0,05 a cada estágio de prosperidade |
| Forja (principal) | `#FF9A4A` | 1,1 ± 0,15 (oscila 6–9 Hz com ruído suave, `ForgeGlow`) | 3,6 | única luz "forte" |
| Brasa/bigorna | `#FFB13B` | 0,6 (pico 1,2 por 80 ms a cada golpe) | 2,4 | |
| Janela | `#BFD4FF` | 0,5 | cone de luz projetado no piso (sprite de "poça de luz" 64 × 32) | luz fria diurna; pode apagar à noite |
| Lampiões (estágios 1–2) | `#FFC77A` | 0,35 | 1,6 | brilho em volta, nunca maior que a forja |

- **Sombras de contato:** elipse `#000000` alfa 90 sob todo personagem (18 × 6 px) e losango sombra sob cada prop (alfa 70, 1 tom mais escuro que o piso). Já existe para o ferreiro; estender aos props.
- Sem sombras projetadas dinâmicas (custo e legibilidade).

---

## 4. Paleta

Paleta mestra de **24 cores**. Todo sprite usa **somente** essas cores (e variações por alfa). Geradores devem referenciá-las por nome em uma classe única `ArtPalette` (Editor), nunca por literal solto.

| Nome | Hex | Papel |
|---|---|---|
| **Tinta** | `#1B1418` | contorno universal, fendas, pupilas |
| **Sombra do mundo** | `#120D12` | vazio fora da loja; fundo da câmera |
| **Pedra escura** | `#3F3A3C` | base de piso de pedra, fundo de paredes de pedra |
| **Pedra média** | `#5E5755` | pedra de forja, soleiras |
| **Pedra clara** | `#8A817A` | topo de pedra, realce |
| **Reboco sujo** | `#8F7E62` | sombra do reboco |
| **Reboco** | `#C9B58F` | parede de reboco (base) |
| **Reboco claro** | `#E2D3AE` | realce do reboco, páginas de papel |
| **Madeira escura** | `#4B2E1B` | vigas, sombra de madeira, cabo gasto |
| **Madeira média** | `#7A4B2B` | tábuas do piso, mesas |
| **Madeira clara** | `#A9713F` | tampo, tábuas novas |
| **Palha / mel** | `#C99A5B` | caixote novo, cesta, realce de madeira |
| **Couro** | `#6E4528` | avental, cintos, livros |
| **Couro claro** | `#9A6A3E` | realce de couro, bolsas |
| **Ferro escuro** | `#3B3F4A` | sombra de metal, face inferior da bigorna |
| **Ferro** | `#5F6573` | metal base |
| **Ferro claro** | `#9AA3B2` | realce de metal |
| **Brilho de aço** | `#D5DBE6` | gume, brilho de 1 px |
| **Carvão** | `#2A1D1A` | fuligem, boca da forja, marcas de queimado |
| **Brasa** | `#FF6A1F` | fogo, brasa, ferro quente |
| **Chama** | `#FFB13B` | miolo do fogo, faíscas |
| **Núcleo** | `#FFE08A` | centro do fogo, crítico, contorno de seleção |
| **Ouro** | `#F2B632` | moedas, ícones de ouro (base) |
| **Ouro claro / sombra** | `#FFD968` / `#A56E14` | realce e sombra do ouro |

**Pele e cabelo** (usados por personagens, fora da contagem de 24 por serem "orgânicos"):

| Nome | Hex |
|---|---|
| Pele | `#E3B089` |
| Pele sombra | `#BE8660` |
| Pele realce / nariz | `#F0C7A3` |
| Cabelo castanho / barba | `#8A5230` (realce `#A5693A`, sombra `#5E361F`) |
| Fuligem na pele | `#6B4A3A` |

**Tecidos por facção / papel** (sempre com 3 tons: luz, base, sombra):

| Facção / papel | Base | Luz | Sombra | Notas |
|---|---|---|---|---|
| **Ferreiro (acento)** | `#B4472E` ferrugem | `#D36A47` | `#7C2E22` | só o ferreiro e seus aprendizes (em tom mais apagado) |
| **Reino / Guarda / Cavaleiro** | `#2F4F8F` azul-real | `#4A72C0` | `#1F3560` | tabardos, estandartes, carta do rei |
| **Aldeões** | `#6F8A4A` verde-erva | `#97B068` | `#4A6032` | e ocres `#B88A3E` |
| **Caçadores / floresta** | `#3E5F3A` verde-floresta | `#5F8A56` | `#2A4228` | capuz, aljava |
| **Mercadores / guilda** | `#6B3A6E` ameixa | `#92568F` | `#4A2650` | roupa e chapéu |
| **Linho (camisa, ataduras)** | `#E0CFA8` | `#F3E9D8` | `#B3A07C` | punhos, aventais claros, olhos |

**Estados de UI:** ouro `#F2B632`; reputação `#6FB7D9` (azul-celeste, único azul-claro da UI); urgência `#D8452F` (vermelho vivo, mais frio que a ferrugem do ferreiro); sucesso `#7FB05A`; texto `#F4E8D2`; texto atenuado `#B8A688`; painel `#2A1D16` / madeira `#4B2E1B` / moldura de metal `#5F6573` + `#9AA3B2`.

**Cores de qualidade/material (ícones):** ferro `#9AA3B2`; aço `#C4D3E8` (tom azulado mais claro, contorno `#3B3F4A`); mithril `#7FE3E0` com sombra `#2E8F9A` e brilho `#E8FFFF`.

---

## 5. O ferreiro (ponto mais importante)

### 5.1 Conceito
**Nome de trabalho:** o Ferreiro (nome a decidir pelo usuário; sugestão **"Mestre Baldo"**, que cabe bem em PT-BR e soa afável).
**Uma frase:** *um homem largo e barbudo de lenço vermelho, avental de couro todo queimado, martelo sempre no ombro.*

**Silhueta-chave (teste de 24 px):** 
1. **Cabeça + barba formam um "triângulo" arredondado** (cabeça larga em cima, barba descendo para o peito) — nada mais no jogo tem isso.
2. **Lenço vermelho** com **duas pontas** que voam para trás: é o único elemento vermelho saturado da cena.
3. **Ombros largos** (20 px de largura de torso vs. 12 px da cintura/pernas): silhueta em "V".
4. **Martelo no ombro**: cabo diagonal e cabeça retangular acima do ombro direito: a assinatura da ferraria.
5. **Avental** mais escuro que a camisa e a pele dos antebraços (contraste).

**Personalidade (guia de pose/animação):** carrega o peso nos ombros; passos firmes e curtos; ao esperar, respira devagar e coça a barba (opcional, P2).

### 5.2 Proporções

Sprite de tela (canvas) **40 × 64 px** (PPU 64, pivô nos pés `(0,5; 0,0625)` = pixel (20, 4)). A altura útil do corpo é **48 px** (do pé ao topo do lenço); sobram 12 px de espaço acima para o martelo levantado e 4 px abaixo para a sombra.

| Parte | Linhas (y, 0 = base do canvas) | Altura | Largura |
|---|---|---|---|
| Sombra de contato | 1–6 | 6 | 18 (elipse) |
| Botas | 4–9 | 6 | 6 cada, 2 px de vão entre elas |
| Calça (visível sob o avental) | 10–17 | 8 | 5 cada |
| Avental (saia) | 18–23 | 6 | 12 |
| Torso + avental (peito) | 24–36 | 13 | 20 (ombros), 14 (cintura) |
| Cabeça (rosto, barba, lenço) | 37–52 | 16 | 16 |
| Martelo (levantado) | até y = 62 | — | cabo 2 px, cabeça 8 × 5 px |

(O corpo termina em y = 52; o canvas de 64 deixa a folga do martelo.) 

### 5.3 Paleta própria

| Parte | Luz | Base | Sombra |
|---|---|---|---|
| Lenço / camisa / acento | `#D36A47` | `#B4472E` | `#7C2E22` |
| Pele (rosto, antebraços) | `#F0C7A3` | `#E3B089` | `#BE8660` |
| Fuligem (marcas na pele) | | `#6B4A3A` | |
| Barba / bigode / cabelo | `#A5693A` | `#8A5230` | `#5E361F` |
| Avental de couro | `#9A6A3E` | `#6E4528` | `#4B2E1B` |
| Marcas de queimado no avental | | `#2A1D1A` | |
| Punhos / linho | `#F3E9D8` | `#E0CFA8` | `#B3A07C` |
| Cinto + fivela | `#C99A5B` | `#4B2E1B` | fivela `#F2B632` |
| Calça | | `#4A4A5E` (cinza-azulado) | `#33334A` |
| Botas | | `#3A2418` com topo `#6E4528` | |
| Cabo do martelo | `#A9713F` | `#7A4B2B` | `#4B2E1B` |
| Cabeça do martelo | `#9AA3B2` (topo) | `#5F6573` | `#3B3F4A` (base) |

**Por que funciona no escuro:** pele, lenço, punhos de linho e barba-realce são todos de luminosidade ≥ 55%; avental e calça ficam escuros, mas separados do piso por 1 px de `#9A6A3E` (couro claro) na borda iluminada e pela barra de linho. A calça passa de azul-acinzentada (frio) para contrastar com o piso marrom.

### 5.4 Cabeça, em detalhe (direção DownRight, 16 × 16 px)

Legenda: `.` transparente · `O` Tinta · `R` lenço base · `r` lenço sombra · `h` cabelo castanho (sombra) · `S` pele · `s` pele sombra · `K` sobrancelha (`#3E2616`) · `W` branco do olho · `P` pupila (Tinta) · `N` nariz (pele realce) · `M` bigode (realce da barba) · `B` barba base · `b` barba sombra.

```text
....OOOOOOOO....   y15  topo do lenço
..OORRRRRRRROO..
.ORRRRRRRRRRRRO.
.ORRRRRRRRRRRRRO
ORrrrrrrrrrrrRRO   nó/faixa do lenço (tom sombra)
OhhhSSSSSSSSSSO.   testa
OhhsKKKKSSKKKKSO   sobrancelhas grossas
OhhsSWPSSSSWPSSO   olhos (2 px: branco + pupila olhando para a direita)
OhhssSSSSSNNSsSO   nariz
OhBBBMMMMMMMBBBO   bigode largo sobre a barba
ObBBBBBBBBBBBBbO   barba cheia
OObBBBBBBBBBBbOO
.OObBBBBBBBBbOO.
..OObBBBBBBbOO..
....OObBBbOO....
......OOOO......   y0  ponta da barba
```

Detalhes não mostrados na grade (aplicar por cima, 1 px cada):
- **Pontas do lenço:** 2 mechas de 3 × 2 px (`R`/`r`) saindo do canto esquerdo do lenço, para trás/baixo; no quadro de caminhada elas oscilam ±1 px.
- **Fuligem:** 2 pixels `#6B4A3A` na bochecha direita (acima da barba) e 1 na testa.
- **Brilho da barba:** diagonal de 3 px `#A5693A` do bigode ao queixo esquerdo.
- **Cicatriz opcional** (P2): 1 × 3 px pele sombra sobre o olho direito.
- **Ao olhar para baixo-esquerda** (espelhado) a mesma cabeça é usada.
- **Para cima (UpRight / UpLeft):** ver 5.6 (sem rosto: lenço com nó e pontas, nuca, barba só como "borda" nas laterais).

### 5.5 Torso, em detalhe (DownRight, 20 × 19 px, do ombro ao fim do avental)

Legenda: `O` Tinta · `R` camisa/acento base · `r` camisa sombra · `C` punho de linho · `S` pele · `s` pele sombra · `k` fuligem na pele · `L` avental base · `l` avental sombra · `d` marca de queimado · `B` cinto · `G` fivela de ouro.

```text
...OOOOOOOOOOOOOO...   ombros (topo)
..ORRRRrSSSSrRRRRO..   gola aberta + pescoço
.ORRRRRLLLLLLRRRRRO.   alças do avental sobre os ombros
ORRRRRRLLLLLLRRRRRRO
ORRRRRLLLLLLLLRRRRRO   peitoral do avental (bib)
ORRRRRLLLLdLLLRRRRRO   1ª marca de queimado
ORRRRRLLLLLLLLRRRRRO   mangas arregaçadas...
OCCCCCLLlLLLLLCCCCCO   ...até o cotovelo (punho de linho)
OSSSSOLLLLLLLLOSSSSO   antebraços fortes (4 px de largura cada)
OSkSsOLLLLdLLLOSsSSO   fuligem (k) e queimado (d)
OSSSsOLLLLLLLLOSSSsO
OSSsOOLLLllLLLOOSsSO   mãos fechadas
..OO.OBBBGGBBBO.OO..   cinto com fivela de ouro
.....OLLLLLLLLO.....   saia do avental
.....OLdLLLLLLO.....
.....OLLLLLlLdO.....
.....OLLLLLLLLO.....
.....OlLLdLLLlO.....
.....OOOOOOOOOO.....   bainha do avental
```

Notas:
- **Antebraços** são o foco de cor da zona central: pele clara contra a camisa vermelha e o avental marrom. 4 px de largura (grossos), cada um com 1 px de `k`.
- **Marcas de queimado `d`** (`#2A1D1A`): 5–6 pixels isolados no avental, em posições fixas (listas no código); nunca aleatórias.
- **Bainha do avental** tem 1 px de `#4B2E1B` (mais escuro) para fechar a forma.
- **Cinto** `#4B2E1B` com fivela `G` e uma pequena bolsa de couro à direita (3 × 4 px) com realce.

### 5.6 As 4 direções

| Direção | Origem | Mudanças |
|---|---|---|
| **DownRight** (referência) | desenhada | rosto 3/4 virado para a direita; martelo no ombro direito (lado da câmera) com a cabeça atrás da cabeça do ferreiro; antebraço esquerdo livre balançando |
| **DownLeft** | **espelho horizontal** de DownRight | o martelo vai para o lado esquerdo (ferreiro vira "canhoto" ao ir para a esquerda: aceito em P0; em P1, redesenhar o martelo depois do espelho para manter a mão direita) |
| **UpRight** | desenhada | de costas 3/4: lenço com nó e 2 pontas visíveis, nuca (pele 3 × 2 px abaixo do lenço), alças do avental cruzadas em "X" nas costas (`L`), barba só como 1 px de `B` nas laterais do rosto, martelo preso ao cinto (cabeça pendurada) |
| **UpLeft** | espelho de UpRight | idem |

### 5.7 Animações

Convenção de nomes (um PNG por quadro, 40 × 64): `smith_{dr|dl|ul|ur}_{idle|walk|strike}_{n}.png`.
Tempo-base de referência; os valores finais ficam em ScriptableObject de animação (nada de número mágico no código).

#### Parado, respirando (2 quadros por direção)
| Quadro | Duração | Mudança |
|---|---|---|
| idle_0 | 600 ms | pose-base |
| idle_1 | 600 ms | peito/ombros +1 px acima; pontas do lenço -1 px; mão do antebraço livre 1 px mais baixa |
| (opcional) idle_blink | 120 ms a cada ~4 s | linha dos olhos vira linha de Tinta |

#### Caminhada (4 quadros por direção, ciclo de 480 ms = 8 fps)
| Quadro | Pés (relativos ao pivô, em px) | Corpo | Braços / martelo | Lenço |
|---|---|---|---|---|
| walk_0 *contato A* | pé A à frente (+4, -2), pé B atrás (-4, +2) | altura normal | antebraço livre à frente (+2), martelo no ombro | pontas para trás |
| walk_1 *passagem* | pés juntos, pé B ergue 2 px | **+1 px** acima (rebote) | antebraço livre no centro | pontas +1 px acima |
| walk_2 *contato B* | pé B à frente (+4, -2), pé A atrás | altura normal | antebraço livre atrás (-2) | pontas para trás |
| walk_3 *passagem* | pés juntos, pé A ergue 2 px | **+1 px** | antebraço livre no centro | pontas +1 px acima |

Regras: a barba balança 1 px na passagem; sombra de contato fica parada (não acompanha o rebote); o ciclo roda em velocidade proporcional ao deslocamento (a cadência é `velocidade_px_por_s / 24` ciclos por segundo, com comprimento de passo de 12 px) para evitar "pés deslizando".

#### Martelada (6 quadros por direção, ~520 ms; só DownRight/UpRight desenhadas, as outras espelham)
| Quadro | Duração | Pose | Gatilho |
|---|---|---|---|
| strike_0 *preparação* | 100 ms | joelhos dobram (corpo -1 px), martelo sai do ombro e vai para trás | |
| strike_1 *levantar* | 80 ms | martelo acima da cabeça (cabeça em y = 60), corpo estica +1 px, braços acima | |
| strike_2 *topo* | 60 ms | martelo inclinado para a frente, barba -1 px (inércia) | |
| strike_3 *golpe* | 40 ms | martelo na bigorna (pontos de contato à frente, ~+10, -4), corpo inclinado +2 px à frente | **emite faíscas** e dispara o pulso de luz da bigorna |
| strike_4 *impacto* | 100 ms (hit-stop) | segura; antebraços tensos (linha de realce), barba +1 px | tremor de câmera só no crítico |
| strike_5 *recuperar* | 140 ms | martelo volta ao ombro, corpo endireita | |

**Variante rápida (cliques em sequência):** se o próximo golpe chega antes do fim, o ciclo reduz para 4 quadros (strike_1 → strike_3 → strike_4 → strike_5, 50/40/60/90 ms). Mantém a sensação de ritmo sem "travar" o clique.

Tamanho total: 4 dir × (2 + 4 + 6) = 48 quadros; desenhando só DR e UR e espelhando: **24 quadros desenhados**.

### 5.8 Como gerar por código
- Novo arquivo `Assets/_Project/Scripts/Editor/SmithArtGenerator.cs`, chamado de `WorldArtGenerator.Generate()`.
- **Sprite como grade ASCII em C#** (`string[]` + dicionário de paleta `char → Color32`), carimbada em uma `Color32[]` em posições fixas. Isso torna o desenho legível, versionável e **determinístico** por construção. Helper compartilhado: `PixelGrid.Stamp(Color32[] canvas, int w, string[] rows, int x, int y, Dictionary<char, Color32> palette)`.
- Cada quadro = função `DrawSmith(dir, anim, frame)` que carimba: sombra → botas/calça → saia → torso → braços → cabeça → martelo → contorno automático (`AddOutline` já existente, com **contorno seletivo** quente do lado da forja).
- Grava em `Assets/_Project/Art/Characters/Smith/`. `WriteSprite` já é idempotente (só reescreve se os bytes mudam).

---

## 6. A loja (cenário)

### 6.1 Paredes (todas as salas)

| Item | Especificação |
|---|---|
| Altura | **72 px** (era 34). Bloco 64 × 104, pivô `(0,5; 16/104)` |
| Composição | Terço inferior: **rodapé de pedra** (24 px, `#5E5755`, juntas `#3F3A3C`). Dois terços superiores: **reboco** `#C9B58F` com sombra `#8F7E62` e 1–2 manchas por segmento |
| Vigas | Pilares de madeira `#4B2E1B` (6 px de largura) a cada 3 células e **viga horizontal** no topo (6 px, `#7A4B2B` com veio). Constrói um "enxaimel" (estilo meio-madeira) simples |
| Variantes (tiles) | `wall_plain`, `wall_window` (janela 20 × 24 px com cruzeta de madeira e luz `#BFD4FF`), `wall_beam` (pilar), `wall_door` (arco de 28 × 44 px, para Depósito e outras salas), `wall_corner` |
| Topo | borda de madeira de 3 px `#A9713F` (não mais "tampo de pedra" cinza) |
| Parede lateral (esquerda e direita) | A face visível muda: a NW mostra a face SE (clara), a NE mostra a face SW (sombra, 1 tom mais escuro) |
| Props de parede | ferramentas penduradas, quadro de avisos, escudo/estandarte, lampião — são sprites separados, colocados em slots por cima da parede |

### 6.2 Piso

| Zona | Material | Tile | Detalhe |
|---|---|---|---|
| Oficina (trabalho) | **pedra irregular** (`#3F3A3C` base, `#5E5755` realce) | 64 × 32 | lajes desiguais de ~3 variantes que alternam; juntas de 1 px `#2A2426`; sem contorno externo |
| Oficina (circulação / loja) | **tábuas de madeira** (`#7A4B2B`, juntas `#4B2E1B`, 1 nó por tábua) | 64 × 32 | tábuas na direção do eixo isométrico X; 3 variantes |
| Depósito | madeira escura e **terra batida** em um canto | 64 × 32 | |
| Alojamento | tábuas com tapete (sprite 128 × 64) | | |
| Perto da forja | variante **chamuscada** (3 tiles) | `#2A1D1A` mistura | marca de fuligem 20 px de raio |
| Bordas | tiles de **soleira** ao longo das paredes: faixa de sombra de 4 px | | dá profundidade; substitui a "colmeia" |

Regra: no máximo **3 variantes** de tile por material, escolhidas por hash determinístico da célula (nada de aleatório por frame).

### 6.3 Cômodo por cômodo

#### Oficina (8 × 8 células, `RectInt(0,0,8,8)`)
Zona de trabalho no canto de trás (NE) e loja na frente (SW). O ferreiro nasce em (3,3).

| Posição | Elemento | Nota |
|---|---|---|
| Parede NE (fundo-direita) | **Forja** em (6,6) com chaminé que sobe pela parede e **fuligem** na parede (mancha escura em leque de 20 px) | luz principal |
| Parede NE | **ferramentas penduradas** (tenaz, martelo, alicate, serra) em um painel de madeira de 40 × 24 px | |
| Parede NE (centro) | **janela** com luz fria (cone de luz no piso) | única fonte fria |
| Parede NW | **prateleira de armas** (3 armas à mostra, mostram o que a loja já produziu) e **quadro de encomendas** (§10.5) | |
| Centro | **Bigorna** em (4,4) sobre um **cepo de madeira** (tora de 14 px) | |
| Ao lado da bigorna | **Tonel de têmpera** (água escura com reflexo) e **esmeril** (roda de pedra com pedal) | |
| Frente-direita | **Balcão** em (6,2): tampo de madeira clara, **balança de pratos** (dois pratos de 8 px), **moedas** em 2 pilhas e **caixa registradora** de madeira/ferro | |
| Frente-esquerda | **Mesa de encomendas** em (2,6) com **livro de pedidos**, **pena e tinteiro**, e o quadro de encomendas na parede acima | |
| Cantos | barris (2), caixotes empilhados (2), saco de carvão, vassoura encostada | quebrar a simetria: nunca 2 barris iguais lado a lado |
| Chão | tapete pequeno (64 × 32) na frente do balcão, **marca de poça de luz** da janela, **cinzas e carvão** aos pés da forja | |
| Mensageiro / porta da frente | porta de madeira 28 × 44 px na parede SW (fachada) quando a rua existir (P2) | |

#### Depósito (6 × 6, `RectInt(9,1,6,6)`; portas em (8,3) e (8,4))
- **Porta:** passagem na parede entre as salas, com **batente de madeira** e **cortina** (tira de pano de reino azul) em vez de simples vazio.
- **Prateleiras altas** (2) com lingotes de ferro empilhados (cinza) e de aço (azulado) conforme o estoque; **barris**, **caixotes** em pilha de 2 alturas, **corda** pendurada, **saco de carvão**.
- **Estoque visível:** a quantidade de lingotes sobre a prateleira reflete o estoque (0 / poucos / muitos: 3 níveis), para reforçar a economia.
- Iluminação: 1 lampião; mais escuro que a Oficina (ambiente 0,35).

#### Alojamento dos aprendizes (próxima sala, ~5 × 5)
- 2–3 **camas de palha** com cobertor na cor de cada aprendiz (§8.2), **baú** aos pés da cama, **mesa pequena** com vela, **janela** pequena e **varal** com panos.
- Cada aprendiz contratado deixa sinais: cama arrumada, ferramenta preferida pendurada.

#### Frente da Loja / Vitrine (ver §15: fachada aberta e avenida)
- **Fachada** com porta dupla, **letreiro** de madeira pendurado com uma bigorna pintada, **vitrine** (prateleira com 3 armas), **degraus** de pedra, **banco** externo. Aqui os clientes **entram** e esperam em fila (máx. 3 sprites).

#### Quintal / Poço (P2)
- Poço de pedra, pilha de lenha, galinha andando (sprite 12 px), cerca. Apenas cenário; ambiente verde-frio.

### 6.4 Estados de prosperidade (3 níveis)

A prosperidade é um inteiro `0..2` derivado das missões concluídas e da reputação (a definir pelo game design). **Não muda a lógica**: ativa ou desativa props em slots predefinidos e muda a luz.

| Elemento | **Nível 0 — Humilde** | **Nível 1 — Estabelecida** | **Nível 2 — Próspera** |
|---|---|---|---|
| Luz ambiente | 0,35 | 0,45 | 0,55 |
| Luzes | só forja + 1 janela | + 2 lampiões de ferro | + candelabro de teto, lampiões de latão |
| Paredes | reboco manchado, 1 viga rachada | reboco limpo, vigas inteiras | reboco + **friso pintado** de azul-real, **estandarte do reino** |
| Piso | pedra irregular nua | + tapete pequeno | + tapete grande com bordado, tábuas polidas |
| Ferramentas | 3 penduradas (enferrujadas: `#7A5A44` mancha) | 6, limpas | 9, **uma ferramenta dourada** de destaque |
| Prateleira de armas | 1 arma (ferro) | 3 armas (ferro, aço) | 5 armas (ferro, aço, **mithril** brilhando) |
| Balcão | tampo riscado, **1 pilha de moedas** | balança nova, 2 pilhas | **cofre** de ferro, balança de latão, 3 pilhas |
| Barris/caixotes | 2 barris, 1 caixote | 3 + 2 empilhados | 4 + 3, **sacos de material** nomeados |
| Mesa de encomendas | tábua e 2 papéis | quadro de avisos com 4 papéis | quadro grande, **selo real** na parede |
| Decoração | — | vaso com planta na janela | **letreiro dourado**, retrato do rei, tapeçaria |
| Fumaça da chaminé | fina | média | com **brasas ocasionais** |

Cada **sala nova** nasce já no estágio atual (nunca "mais pobre" que a Oficina).

---

## 7. Estações e props

Pegadas em células: **"1 × 1" = 64 × 32 px de base**, as mesmas do layout atual (`WorldDataSeeds`); alturas só crescem, a pegada do jogo **não muda** salvo indicação.

| Objeto | Canvas (px) | Pegada | Silhueta-chave | Cores | Detalhe único | Prio. |
|---|---|---|---|---|---|---|
| **Forja** | 64 × 120 | 1 × 1 | bloco baixo de pedra + **capelo cônico + chaminé alta** que sobe e encosta na parede | pedra `#5E5755`, brasa `#FF6A1F`/`#FFB13B`, carvão | boca em arco com brasa pulsante (2 quadros, 400 ms), **mancha de fuligem** na parede e fole de couro ao lado (12 × 14 px) | P0 |
| **Bigorna** | 40 × 32 | 1 × 1 (centro) | corpo em "T" com **chifre à esquerda** e **calcanhar à direita**, sobre **cepo de madeira** de 14 px | ferro `#5F6573`, topo `#9AA3B2`, cepo `#7A4B2B` | **face de aço brilhante** (1 px `#D5DBE6`) e marcas de golpes (3 pixels de Carvão) | P0 |
| **Tonel de têmpera** | 32 × 36 | 1 × 1 (metade) | barril baixo de aros de ferro, boca aberta | madeira `#7A4B2B`, aros `#3B3F4A`, água `#2A3350` | reflexo da forja na água (2 px `#FFB13B`), **vapor** quando se tempera | P0 |
| **Esmeril** | 36 × 40 | 1 × 1 (metade) | **roda de pedra circular** em armação de madeira em "A" | pedra `#8A817A`, madeira | faíscas de 3 px quando em uso; pedal | P1 |
| **Mesa de encomendas (mensageiro)** | 64 × 56 | 1 × 1 | **tampo inclinado** (escrivaninha) + livro aberto + vela | madeira `#7A4B2B`, papel `#E2D3AE`, selo `#D8452F` | **tinteiro e pena** (3 px) | P0 |
| **Quadro de encomendas** | 48 × 40 (na parede) | slot de parede | painel de madeira com **4–6 papéis** sobrepostos e pregos | `#A9713F`, papéis `#E2D3AE` | cada papel aberto = 1 encomenda; **papel vermelho** = urgente | P0 |
| **Balcão** | 64 × 52 | 1 × 1 | bloco comprido com **tampo saliente**, **balança de dois pratos** (silhueta em "T") | madeira clara, latão `#F2B632` | **moedas** em pilhas (3 níveis), sino de mão | P0 |
| **Mesa de melhorias** | 64 × 52 | 1 × 1 | bancada com **gaveteiro e bigorna pequena** | madeira + ferro | ferramentas e moldes (esboços) | P1 |
| **Prateleira de armas** | 64 × 88 | 1 × 1 (encostada na parede) | estante vertical de 3 prateleiras, armas inclinadas | madeira escura + aço | cada slot mostra uma arma do inventário (§9) | P0 |
| **Painel de ferramentas** | 40 × 28 (parede) | slot | **tenaz em X, martelo, serra** pendurados | ferro + madeira | silhueta clara contra a parede | P0 |
| **Barril** | 28 × 34 | 1 × 1 (metade) | cilindro com 2 aros | `#7A4B2B`/`#3B3F4A` | tampa com 2 variantes (aberta/fechada) | P0 |
| **Caixote** | 32 × 30 | 1 × 1 (metade) | cubo com **tábuas em X** | `#C99A5B`/`#7A4B2B` | pilha de 2 (variante) | P0 |
| **Saco de carvão** | 24 × 22 | meia | saco amassado e amarrado | linho `#B3A07C`, carvão | pedaços de carvão escapando | P1 |
| **Depósito (estoque)** | 64 × 88 | 1 × 1 | prateleira com **lingotes empilhados** | ferro, aço | 3 níveis de estoque (§6.3) | P1 |
| **Cama de palha** | 64 × 36 | 1 × 1 | retângulo baixo com travesseiro | palha, cobertor por aprendiz | contorno colorido | P2 |
| **Porta** | 28 × 44 | slot | arco de madeira com ferragem | madeira escura, ferro | batente e dobradiças visíveis | P1 |
| **Janela** | 20 × 24 | slot | cruzeta de madeira, vidro `#BFD4FF` | | luz projetada no piso (poça 64 × 32, alfa 40%) | P0 |
| **Lampião** | 12 × 20 | slot (parede) | caixa de ferro com chama | ferro + `#FFC77A` | chama oscilando, 2 quadros | P1 |
| **Tapete** | 64 × 32 / 128 × 64 | piso | retângulo com bordado | azul-real/ocre | franjas | P1 |
| **Vassoura, balde, lenha** | 8–20 px | decoração | silhuetas encostadas | | pistas de vida | P2 |
| **Barreira "à venda"** | 64 × 60 | 1 × 1 | tábuas cruzadas + **placa "à venda"** | madeira + papel | placa com preço | P1 |

---

## 8. Clientes e NPCs

Regra de leitura: cada NPC é identificado a distância por **silhueta + uma cor de facção + um acessório**. Altura: 40–54 px (ferreiro = 48). Canvas 32 × 56. Sempre há sombra de contato e contorno de Tinta. Animações: parado (2 quadros) e caminhada (4), todas em DownRight (+ espelho). As falas ficam em balão de texto (UI), não no sprite.

### 8.1 Clientes (silhueta e ficha)

| NPC | Altura | Silhueta | Cor de facção | Acessório | Personalidade (pose) | Prio. |
|---|---|---|---|---|---|---|
| **Guarda da vila** | 50 | tronco reto, **capacete de nasal** + lança vertical (3× a altura da cabeça) | azul-real `#2F4F8F` | lança, escudo pequeno nas costas | postura rígida, parado com a lança | P1 |
| **Caçador** | 46 | **capuz pontudo**, aljava e **arco** diagonal nas costas | verde-floresta `#3E5F3A` | pena vermelha no capuz, arco | levemente curvado, descontraído | P1 |
| **Aldeã** | 44 | **vestido largo (trapézio)**, lenço branco na cabeça, **cesto** no braço | verde-erva/ocre, lenço `#F3E9D8` | cesto com pão | balança o corpo ao andar | P1 |
| **Mercador** | 44 (largo: 28 px) | **barriga**, **chapéu de aba larga**, saco de moedas | ameixa `#6B3A6E` | saco com `$`-moeda dourada, bengala | passo lento, mãos nas costas | P1 |
| **Cavaleiro** | 54 | **armadura completa**, **pluma** no elmo, capa | aço `#9AA3B2` + azul-real | espada na cintura, capa que bate | ombros largos, queixo erguido | P2 |
| **Aprendiz** | ver §8.2 | | ferrugem apagada | | | P0 |

Cada cliente tem **retrato** 48 × 48 px (busto) para a UI de encomenda (P1).

### 8.2 Aprendizes com nome (proposta, poucos e marcantes)

| Nome | Silhueta | Cores | Diferencial | Função |
|---|---|---|---|---|
| **Bento** | **alto e magro**, cabelo louro de espeto | túnica cinza `#8A817A`, avental ferrugem apagado `#8F4A3A` | sorriso grande, martelo pequeno | ajuda a forjar |
| **Lia** | **baixa**, trança escura comprida, lenço verde-erva `#6F8A4A` | avental cinza-claro | carrega um caderno | cuida das encomendas / balcão |
| **Otto** | **troncudo**, careca, orelhas grandes, barba curta ruiva | avental de couro escuro | fole nas costas | cuida da forja e do carvão |

Cada um trabalha em sua estação (cena viva): Bento na bigorna secundária, Lia na mesa, Otto no fole. Quando um aprendiz é contratado, o sprite nasce com **poeira de construção** e **balão de saudação**. Substitui os 4 "bonecos" iguais de hoje.

---

## 9. Armas e itens

### 9.1 Ícones (32 × 32) e sprites no mundo

| Arma | Silhueta do ícone (32 × 32, diagonal ↗) | Comprimento no mundo | Detalhe reconhecível |
|---|---|---|---|
| **Adaga** | lâmina curta e larga (6 px) + guarda pequena | 14 px | punho de couro com pomo redondo |
| **Espada curta** | lâmina média reta + guarda em T | 22 px | gume com 1 px de `#D5DBE6` |
| **Lança** | haste longa (≥ 28 px) + ponta de folha | 40 px | borla/pano vermelho sob a ponta |
| **Machado** | cabo reto + **meia-lua** de ferro de um lado | 24 px | gume curvo brilhante |
| **Escudo** | **círculo ou escudo heráldico** com boss central | 22 px | umbo (boss) com brilho e aro de ferro |
| **Espada longa** | lâmina longa e fina + **guarda larga + pomo** | 34 px | sulco central (1 px mais escuro) |

- **Sprite no mundo:** o mesmo desenho a 1:1 (sem escala), deitado ou inclinado a 30° em prateleira/balcão; ícone de UI: o mesmo desenho a 32 × 32 com contorno de Tinta.
- Geração: cada arma é uma função `DrawWeapon(kind, material, quality)` com grade ASCII por tipo e **troca de paleta por material**.

### 9.2 Materiais

| Material | Rampa de cor | Distintivo visual |
|---|---|---|
| **Ferro** | `#3B3F4A` / `#5F6573` / `#9AA3B2` | cinza fosco, 1 px de brilho |
| **Aço** | `#3B3F4A` / `#7F8FA6` / `#C4D3E8` / brilho `#F3F8FF` | azulado, brilho largo (2 px) |
| **Mithril** | `#2E8F9A` / `#7FE3E0` / `#E8FFFF` | **cintilação** (2 quadros, 600 ms) e leve tom lilás nas sombras `#5A4E9A` |

### 9.3 Qualidade (1–5 estrelas)

- **Ícone:** fileira de 1–5 **estrelas de 5 × 5 px** sob a arma (`#F2B632`, vazias `#3F3A3C`). Moldura do ícone muda: 1–2 estrelas = moldura de couro; 3 = ferro; 4 = prata; **5 = ouro com brilho animado** (estrela passando, 4 quadros).
- **No mundo:** qualidade 1 = silhueta levemente torta (1 px de deslocamento), lâmina com **manchas de ferrugem**; qualidade 3 = limpa; qualidade 5 = **brilho** de 1 quadro a cada ~3 s e **selo do ferreiro** (marca) no cabo.
- **Texto:** estrelas sempre ao lado do nome, nunca só por cor (daltonismo).

---

## 10. UI

### 10.1 Linguagem visual
- **Painéis:** "couro e madeira com moldura de metal". Fundo `#2A1D16` (opaco ≥ 92%), borda interna de 2 px de `#4B2E1B`, **moldura externa de metal** 2 px (`#5F6573` com realce `#9AA3B2` em cima e à esquerda), **quatro cantos com rebites** de 3 × 3 px. Cantos **retos** (ou chanfrados de 2 px), não arredondados, para combinar com pixel art.
- Para fazer isso no UI Toolkit sem assets externos: gerar por código **nine-slice** `panel_frame.png` (24 × 24, bordas de 8) e `button_*.png` no Editor, e usar `-unity-slice-*` no USS. Textura: `Assets/_Project/UI/Art/`.
- **Botões:** 3 estados (normal, hover, pressionado) de madeira clara/latão: normal `#7A4B2B` com borda `#A9713F`; hover `#A9713F`; pressionado desloca o texto 1 px; desabilitado `#3F3A3C` com texto atenuado. Botão primário (comprar/entregar) em **ouro** `#F2B632` com texto escuro `#2A1D16`.
- **Ícones:** 16 × 16 e 24 × 24, estilo pixel, contorno de Tinta (ouro, reputação, urgência, estrela, relógio, martelo, saco, carta, balança).
- **Tipografia:** hoje é a fonte padrão sem personalidade. Proposta: uma **fonte bitmap 5 × 7** gerada por código (letras maiúsculas e minúsculas em PT-BR com acentos) ampliada a 2×; **ou**, se o usuário autorizar, uma fonte pixel OFL (pergunta aberta 2). Enquanto isso, na fonte padrão: **mínimo 14 px** para qualquer texto, 18 px para valores, **sempre com contorno de 1 px** `#1B1418` (`-unity-text-outline`) ou sombra.
- **Contraste:** texto `#F4E8D2` sobre `#2A1D16` ≥ 10:1. Texto atenuado `#B8A688` apenas em informação secundária, nunca menor que 13 px. Nada de opacidade 70% em dicas.

### 10.2 Cores de estado

| Estado | Cor | Uso |
|---|---|---|
| Ouro | `#F2B632` | saldo, preços, recompensas |
| Reputação | `#6FB7D9` | barra e valor de reputação (ícone de coroa/estandarte) |
| Urgência | `#D8452F` | prazo curto, papel vermelho, relógio em contagem |
| Sucesso | `#7FB05A` | missão cumprida, qualidade ok |
| Neutro | `#B8A688` | informações secundárias |
| Seleção | `#FFE08A` | contorno de item/estação ativa |

### 10.3 HUD (adaptação do `HUD.uss` atual)

| Elemento atual | Mudança |
|---|---|
| `.panel` (translúcido, `border-radius: 10px`) | opaco, nine-slice de moldura de metal; remover `border-radius`; `padding` 12 → 14 |
| `.gold-icon` (círculo chapado) | ícone de **moeda de ouro em pixel art** 20 × 20 (com realce e aro) |
| `.gold-label` 34 px | manter, adicionar contorno e seta de ganho "+5" que sobe e some |
| `.caption` 14 px "ouro" | trocar por ícone de moeda; sem legenda de 11 px |
| `.forge-panel` (barra laranja arredondada) | barra de progresso com **ferro quente**: a barra vai de `#5F6573` para `#FF6A1F` conforme enche, com **brilho** e ponta quente; moldura de ferro |
| `.shop-tab` / `.buy-amount` | abas como **abas de madeira** (a ativa levanta 2 px); botões quadrados |
| `.shop-row__buy` | botão de madeira/ouro com ícone da moeda |
| `.floating-text` | contorno 2 px; **cor por tipo** (ouro/reputação/crítico); duração 0,9 s; **fundo** com sombra para ler sobre a parede clara |
| `.hint` (opacidade 0,7) | opacidade 1; texto 14 px com contorno |
| `.station-prompt` | **pergaminho** pequeno com ícone da tecla "E" |

### 10.4 Novas telas (visual)
- **Carta do rei:** pergaminho (`#E2D3AE`, bordas rasgadas por código) com **selo de cera vermelho** `#D8452F`, cabeçalho com estandarte azul-real e retrato 48 × 48 do rei; fonte maior (20 px); botões "Aceitar" (ouro) e "Depois" (madeira).
- **Cartão de encomenda de cliente:** cartão de papel de 220 × 120 px com **retrato do cliente**, item pedido (ícone 32 × 32 + estrelas mínimas), prazo (relógio, vermelho abaixo de 20%), recompensa (ouro + reputação). Selo de "urgente" nas pontas.
- **Balcão / venda:** lista de itens em prateleiras de madeira; cada linha: ícone, nome, estrelas, **preço**; um clique vende; sem listas gigantes.

### 10.5 Quadro de encomendas
Elemento duplo: **objeto no mundo** (§7) e **painel**. O painel mostra de 3 a 6 **papéis presos** em um quadro de cortiça (`#C99A5B` base); cada papel é um cartão (acima) levemente girado (±2 px, com `Rect` fixo, sem rotação fracionária); papéis concluídos ganham **carimbo verde "ok"**; novos balançam 1× ao chegar.

---

## 11. Efeitos

Todos com **sprites gerados por código** (texturas pequenas em `Art/Fx/`) usados pelo `ParticleSystem`, no mesmo material `Sprite-Unlit`, ordem `FX`. Cores só da paleta.

| Efeito | Textura (px) | Parâmetros-chave | Cores | Prio. |
|---|---|---|---|---|
| **Faíscas** | `spark_px` 3 × 3 (cruz) e 2 × 2 | vida 0,2–0,5 s, velocidade 2,5–7, gravidade 1,6, 8–14 partículas (crítico 28), esticadas com `lengthScale 1,5` | `#FFE08A` → `#FF6A1F` → transparente | P0 |
| **Brasa flutuante** | `ember` 2 × 2 | sobem 0,3 u/s, vida 1,5 s, 1/s da forja | `#FFB13B` → `#FF6A1F` | P0 |
| **Fumaça da chaminé** | `smoke` 3 tamanhos 12/16/20 px com **dithering ordenado** nas bordas | emite 0,6/s, sobe 0,4 u/s, cresce ×1,6, alfa 0,5 → 0, vento -0,1 | `#5E5755`/`#8A817A` | P0 |
| **Pulso de luz da bigorna** | (luz) | +0,6 de intensidade por 80 ms a cada golpe, 2× no crítico | `#FFB13B` | P0 |
| **Vapor da têmpera** | `steam` 3 quadros 16 px | burst de 6, sobe 0,6 u/s, vida 0,8 s | `#D5DBE6` alfa 0,6 | P1 |
| **Poeira de construção** | `dust` 12 px, borda dithering | já existe `BuildDust` (14 partículas); trocar por sprites; adicionar **2 tábuas** voando | `#C9B58F`, `#8F7E62` | P1 |
| **Brilho de qualidade** | `shine` estrela de 4 pontas 9 × 9, 4 quadros | atravessa a arma 1× a cada 3 s se ≥ 4 estrelas; 5★: + partículas | `#F3F8FF`/`#FFE08A` | P1 |
| **Moedas / brilho de ouro** | `coin` 4 quadros 8 × 8 (giro) | ao vender: 3 moedas pulam em arco até o saldo | `#F2B632`, `#FFD968` | P1 |
| **Poeira de passos** | `puff` 6 px | 2 por passo, só em terra/piso poeirento | `#8F7E62` alfa 0,4 | P2 |
| **Papel/poeira ao entregar** | 8 × 8 | selo estoura com 6 papeizinhos | `#E2D3AE` | P2 |

---

## 12. Lista de assets priorizada

**Convenções de geração:** tudo em `Assets/_Project/Scripts/Editor/`; função `Generate()` chamada por `PlaceholderArtGenerator.Generate()` (que já faz parte de `RunAllBatch`); saída determinística (sem `Random` não semeado; hash por célula/pixel) e **idempotente** (`WriteSprite` só reescreve se os bytes mudarem). Esforço: **S** ≤ 2 h, **M** 2–6 h, **L** > 6 h.

### P0 — primeira passada visual (ferreiro + Oficina)

| # | Asset | Como gerar | Arquivo / função | Esf. |
|---|---|---|---|---|
| 0.1 | **Paleta única** `ArtPalette` + utilitários de grade (`PixelGrid.Stamp`, `Outline`, `OrderedDither`) | classe estática | novo `ArtPalette.cs`, `PixelGrid.cs` | S |
| 0.2 | **Ferreiro: 4 direções × idle (2)** | grades ASCII §5.4/5.5 + pernas/pés | novo `SmithArtGenerator.cs` → `DrawSmith(dir, "idle", n)` | M |
| 0.3 | **Ferreiro: caminhada (4) × DR/UR** + espelhos DL/UL | idem, variação de pernas/braços | `SmithArtGenerator.DrawSmith(.., "walk", n)` | M |
| 0.4 | **Ferreiro: martelada (6) × DR/UR** + espelhos | idem | `SmithArtGenerator.DrawSmith(.., "strike", n)` | L |
| 0.5 | **Paredes 72 px** (plain, janela, viga, canto) + poça de luz da janela | blocos com reboco/rodapé/viga | `WorldArtGenerator.WriteWallSet()` (substitui `CreateWallTiles`/`WallSprite`) | M |
| 0.6 | **Piso de pedra e madeira** (3 variantes cada, sem contorno) | tile 64 × 32 com juntas desenhadas | `PlaceholderArtGenerator.DrawFloorTile()` reescrita + `FloorTileVariants()` | M |
| 0.7 | **Forja com chaminé + fuligem** (2 quadros de brasa) | bloco 64 × 120 + animação por `Sprite[]` | `WorldArtGenerator.DrawForge()` (substitui `ForgeDetails`) | M |
| 0.8 | **Bigorna sobre cepo**, 64 PPU, 40 × 32 | grade ASCII | `PlaceholderArtGenerator.DrawAnvil()` reescrita; **ajustar** `StationPrefabFactory.BuildAnvil` (`scale 0.5` → 1) | S |
| 0.9 | **Balcão com balança e moedas** | grade + moedas 3 níveis | `WorldArtGenerator.DrawCounter()` | M |
| 0.10 | **Mesa de encomendas + livro** e **quadro de encomendas** (parede) | grade | `WorldArtGenerator.DrawOrderDesk()`, `DrawBoard()` | M |
| 0.11 | **Props**: barril, caixote, painel de ferramentas, prateleira de armas, tonel de têmpera | grades por objeto | `PropArtGenerator.cs` (novo) | L |
| 0.12 | **Luz**: ambiente 0,45 + poça da janela + oscilação da forja | `Light2D` global e `ForgeGlow`; parâmetros na prefab | `StationPrefabFactory` + cena (`WorkshopSceneBuilder`, pedir ao agente principal) | S |
| 0.13 | **Faíscas, brasas e fumaça** (texturas) | pequenas texturas determinísticas | `FxArtGenerator.cs` (novo) | S |
| 0.14 | **Aprendizes nomeados** (Bento, Lia, Otto) idle + trabalho (2 quadros) | grades curtas | `SmithArtGenerator.DrawApprentice(name, frame)` | M |

### P1 — a loja respira

| # | Asset | Como gerar | Esf. |
|---|---|---|---|
| 1.1 | **Estados de prosperidade** (props por nível) em slots | sprites de props alternativos (`prop_*_tier1/2`) | M |
| 1.2 | **Depósito:** porta com cortina, prateleiras com estoque (3 níveis), barris/caixotes | `PropArtGenerator` | M |
| 1.3 | **Armas: 6 ícones + sprites** × 3 materiais | `WeaponArtGenerator.DrawWeapon(kind, material, quality)` | L |
| 1.4 | **Qualidade (estrelas + moldura + brilho)** | `WeaponArtGenerator.DrawFrame(quality)` | S |
| 1.5 | **Clientes:** guarda, caçador, aldeã, mercador (idle + caminhada) | `NpcArtGenerator.cs` (grades) | L |
| 1.6 | **UI nine-slice:** painel, botão (3 estados), aba, barra de forja | `UiArtGenerator.cs` → `Assets/_Project/UI/Art/`; ajustar `HUD.uss` | M |
| 1.7 | **Ícones de UI** (ouro, reputação, urgência, estrela, relógio, carta) 16 e 24 px | grades | M |
| 1.8 | **Esmeril, mesa de melhorias, lampião, janela dupla, tapete, porta** | `PropArtGenerator` | M |
| 1.9 | **Vapor, poeira nova, brilho, moedas** | `FxArtGenerator` | S |
| 1.10 | **Alojamento:** camas, baú, mesa | `PropArtGenerator` | M |
| 1.11 | **Parede lateral fina** (laje de 8 px em vez de bloco): resolve a "caixa" | `WorldArtGenerator.WriteThinWalls()` | L |

### P2 — acabamento

| # | Asset | Esf. |
|---|---|---|
| 2.1 | Retratos 48 × 48 de clientes e do rei | L |
| 2.2 | Cavaleiro, mercador viajante, evento | M |
| 2.3 | Fachada da loja (porta dupla, letreiro, vitrine, rua) | L |
| 2.4 | Quintal (poço, lenha, galinha) | M |
| 2.5 | Fonte bitmap gerada por código (se o usuário não liberar fonte OFL) | L |
| 2.6 | Animações extras do ferreiro (coça a barba, comemora, bebe água) | M |
| 2.7 | Martelo redesenhado após o espelho (ferreiro destro nas 4 direções) | S |
| 2.8 | Variações de piso/parede por sala; efeitos de clima e hora do dia | L |

---

## 13. Critérios de aceitação da primeira passada (P0)

Captura de **1280 × 720** da Oficina, estágio 0, zoom 2×, jogador na posição inicial, e outra no **modo forja** (3×). Tudo abaixo é verificável olhando a imagem.

**Ferreiro**
- [ ] O ferreiro é identificado em < 1 s sem legenda: **lenço vermelho, barba grande, avental escuro e martelo no ombro** visíveis.
- [ ] Em uma versão reduzida a 24 px de altura, ainda aparecem o **vermelho do lenço** e a **massa marrom da barba**.
- [ ] O ferreiro **não se perde** no piso: ao menos 3 regiões (lenço, pele/antebraço, punho de linho) claramente mais claras que o piso ao redor.
- [ ] Altura 48 px; **ombros mais largos que a cintura**; cabeça ≈ 1/3 da altura.
- [ ] As 4 direções existem e a transição entre elas não "pula" (pivô nos pés, os pés não deslizam > 1 px).
- [ ] Caminhada com 4 quadros e martelada com 6 quadros visíveis (verificável em sequência de capturas; faíscas surgem no quadro de golpe).

**Cena**
- [ ] A **forja** é o ponto mais claro/quente da imagem; **mais nada** (exceto faíscas e ouro) usa `#FF6A1F`.
- [ ] As paredes têm **viga, janela com luz fria e reboco** (não são "caixas cinza"); altura de parede ≥ 1,4× a do ferreiro.
- [ ] O piso mostra **tábuas ou lajes**, sem a "colmeia" de contornos pretos.
- [ ] **Teste da silhueta:** forja, bigorna, balcão, mesa de encomendas, barril e caixote são distinguíveis em preto sólido.
- [ ] Há ≥ 8 **props de vida** (ferramentas penduradas, barris, caixotes, prateleira, quadro, saco de carvão...) e nenhuma área de piso nua > 2 × 2 células.
- [ ] A **bigorna** tem ~40 px de largura contra ~20 px do corpo do ferreiro (no máximo 2×, hoje 2,5×–3×).
- [ ] **Sombra de contato** sob cada personagem e prop.
- [ ] **Pixel perfeito:** todos os pixels de arte têm o mesmo tamanho na tela (zoom inteiro); sem bordas serrilhadas irregulares.
- [ ] A luz ambiente deixa ver os cantos (mínimo ~20% de luminosidade no piso mais escuro).

**UI**
- [ ] Todo texto ≥ 14 px, com contraste ≥ 7:1 sobre o fundo e contorno de 1 px.
- [ ] Painéis com moldura de metal e rebites (nine-slice); sem cantos arredondados de web.
- [ ] A moeda do saldo é um **ícone** de moeda, não um círculo chapado.
- [ ] Números flutuantes legíveis sobre parede clara e escura.

**Pipeline**
- [ ] Rodar `RunAllBatch` duas vezes seguidas **não altera** nenhum PNG (idempotência: `git status` limpo na segunda execução).
- [ ] Nenhuma cor fora de `ArtPalette`.

---

## 14. Decisões do usuário (2026-10-06)

| # | Tema | Decisão |
|---|---|---|
| 1 | Câmera e escala | **Zoom inteiro** (2× padrão, 3× no modo forja). Ferreiro com **48 px** de altura (canvas 40×64, pivô nos pés) |
| 2 | Fonte da UI | **Fonte pixel de licença livre (OFL)**, por exemplo *Pixelify Sans*. É um arquivo de terceiros: só entra no projeto com a licença junto (`Assets/_Project/UI/Fonts/`) e depois de baixada com a aprovação explícita do usuário no momento |
| 3 | Protagonista | **Fixo:** *Mestre Baldo*, barbudo, sem personalização |
| 4 | Aprendizes | Nomes do GDD v2 (**Tomás**, **Inês**, **Bento**), no máximo 3. Os nomes sugeridos antes (Bento, Lia, Otto) ficam descartados |
| 5 | Prosperidade e luz | Os 3 estágios (Humilde, Estabelecida, Próspera) sobem com **capítulos e melhorias concluídos**. A luz da janela acompanha o **ciclo dia/noite** do `DayClock` (manhã, tarde, noite, com lampiões acendendo) |

### Pendências técnicas para o agente principal (não são do diretor de arte)
- Câmera com zoom inteiro (`CameraFollow`): degraus 2× e 3×, e o foco do modo forja em 3×.
- `StationPrefabFactory.BuildAnvil`: a escala 0,5 passa a 1 quando a bigorna for redesenhada a 64 PPU.
- `WorkshopSceneBuilder`: luz ambiente `#2B3350` a 0,45.
- Reprodutor de quadros de animação que carregue sprites por quadro, com nomes `smith_{dr|dl|ul|ur}_{idle|walk|strike}_{n}.png`.
- Iluminação por hora do dia (depende do `DayClock`, M4).

---

## 15. A avenida e a vitrine

> A loja deixa de ser uma caixa fechada: a borda da **frente-direita (y = -1)** vira **fachada aberta** para uma **avenida** por onde as pessoas passam. O ferreiro continua dentro; a rua é vista, habitada e é de onde chegam os clientes ao balcão. Esta seção complementa §6 (a "Frente da Loja" do §6.3 passa a ser esta fachada) e §8 (clientes).

### 15.0 Geometria (referência)

Projeção usada nas contas: tela `sx = (x - y) × 32`, altura `up = (x + y) × 16` (px de mundo; 1 célula em x = 32 px à direita e 16 acima; 1 célula em y = 32 px à esquerda e 16 acima).

| Zona | Células | Papel | Altura do solo |
|---|---|---|---|
| Interior (Oficina) | x 0–7, y 0–7 | já existente | 0 |
| **Soleira** | y = -1, x 0–7 | borda da fachada: batente, parapeito, janela de atendimento | degrau de 4 px |
| **Calçada da loja** (Z1) | y = -2, x -6–14 | passeio elevado, vitrine, fila do balcão | +6 px |
| **Rua** (Z2) | y = -3 a -5, x -6–14 | calçamento, pista de pedestres e carroças | 0 |
| **Calçada oposta** (Z3) | y = -6, x -6–14 | barracas e cenário de fundo | +6 px |
| **Vizinhos** (Z3) | x -6 a -3 e x ≥ 15, y ≥ 0 | fachadas baixas na mesma linha y = -1 | 0 |

**Ajuste proposto:** manter **x de -6 a 14 e y de -6 a -2** (21 × 5 = 105 células de rua). Cinco linhas é o mínimo que comporta calçada, 3 pistas (ida, meio, volta) e calçada oposta; mais que isso gastaria arte que não aparece em 1280 × 720. Os vizinhos precisam ser **baixos (≤ 40 px)**: uma casa de 64 px em (-2, 4) cobriria o piso do canto esquerdo da Oficina; com 40 px e recuada para x ≤ -3 não cobre nada.

**Layout (pedido ao agente principal; decisão de gameplay):** o **balcão** vai de (6,2) para **(6,0)**, encostado na fachada, com a **janela de atendimento** na célula de fachada (6,-1). O **cliente atendido** fica em (6,-2) e a **fila** ocupa (5,-2) e (4,-2). O Depósito (9–14, 1–6) tem a frente em y = 0, e a calçada continua diante dele.

### 15.1 Fachada / vitrine da loja

**Leitura de "loja aberta" em 1 s** (pistas por ordem de peso visual):
1. **Toldo listrado** ferrugem `#B4472E` e linho `#E0CFA8` (listras de 8 px): a maior forma saturada da fachada e a assinatura da loja. **Nenhuma outra barraca da rua usa ferrugem.**
2. **Placa pendurada** com a silhueta de uma bigorna em ouro `#F2B632`, em suporte de ferro; balança em 2 quadros (±1 px a cada 2 s).
3. **Luz quente escapando** da loja: poça de luz de 64 × 32 px (`#FFB070`, alfa 45%) por célula de soleira e calçada, até y = -3.
4. **Mostruário** de armas inclinadas sobre o parapeito, visível de fora.
5. **Plaquinha "ABERTO"** (16 × 10 px) pendurada na janela de atendimento; vira "FECHADO" fora do expediente.

**Módulos ao longo de y = -1** (pivô no centro da pegada, PPU 64):

| x | Módulo | Canvas (px) | Descrição |
|---|---|---|---|
| 0 | `facade_post_corner` | 64 × 104 | pilar de madeira escura `#4B2E1B` de 8 px de largura, base de pedra `#5E5755` de 14 px, 72 px de altura |
| 1–2 | `facade_sill_display` | 64 × 70 | **parapeito de 18 px** (madeira `#7A4B2B`, tampo `#A9713F`) com mostruário: espada curta e escudo em suporte; sem vidro |
| 3 | `facade_halfdoor` | 64 × 84 | **meia-porta**: metade de baixo fechada (24 px), metade de cima aberta, dobradiças de ferro; só decoração |
| 4–5 | `facade_sill_tools` | 64 × 70 | parapeito de 18 px com **ferramentas** (tenaz, martelo) e 1 barril de apoio |
| 6 | `facade_counter_window` | 64 × 90 | **janela de atendimento:** tampo saliente (projeta 6 px para a rua), **balança de pratos** (latão `#F2B632`), pilha de moedas, sino; lanterna de ferro (`#FFC77A`) em suporte |
| 7 | `facade_post_end` | 64 × 104 | pilar final com base de pedra |
| 0–7 (alto) | `facade_lintel` | 64 × 14 por célula | **viga** de madeira `#7A4B2B` a 66–72 px de altura (8 px de espessura), ligada aos pilares; prende a placa e o toldo |
| 1–7 (alto) | `facade_awning` | 64 × 40 por célula | toldo inclinado: fixa a 64 px, cai 20 px para fora até 44 px, **franja** de 4 px na ponta, sombra de 12 px no piso |

**Legibilidade do interior (regras):**
- Só as bordas y = -1 e x = -1 ganham elementos novos; as paredes de fundo (x = 8 e y = 8, 72 px) seguem o §6.1, com janela e viga.
- Tudo o que fica à frente do interior tem **no máximo 18 px** entre os pilares. Os únicos elementos altos são os **pilares (8 px)**, a **viga (8 px)** e o **toldo**. Quando o ferreiro está a até 2 células atrás do toldo, este **recolhe para 12 px** (fade de 0,25 s), para ele nunca ficar escondido.
- **Parede lateral da frente-esquerda (x = -1):** **muro baixo de pedra de 28 px** com **colunas** de 8 px a cada 2 células (até 56 px) e **trepadeira** (manchas `#6F8A4A` de 3 px). Mostra o interior e separa o vizinho.
- Sombra de contato nos pilares e no parapeito: losango com 70% de alfa, 6 px.

**Geração:** `FacadeArtGenerator.cs` (novo, Editor), com `Generate()` chamado por `WorldArtGenerator.Generate()`; funções `DrawPost`, `DrawSill(variant)`, `DrawCounterWindow`, `DrawLintel`, `DrawAwning(tier)`, `DrawHangingSign(tier, frame)`, `DrawSideWall`. Grades ASCII (§5.8) e `PixelGrid.Stamp`. Listras do toldo por `((x + y / 2) / 8) % 2`, sem aleatoriedade. **Esforço L, prioridade P0** (sem fachada não existe a ideia de "loja aberta").

### 15.2 A avenida

| Elemento | Tamanho (px) | Especificação | Prio. |
|---|---|---|---|
| **Paralelepípedos** | tile 64 × 32, 3 variantes | pedras de 8 × 5 px em fileiras deslocadas (~4 por tile); 3 tons (`#3F3A3C`, `#5E5755`, e `#8A817A` em ~15% das pedras); juntas `#2A1D1A` de 1 px. **Sem ruído:** a variante vem de `hash(x, y) % 3` e a cor de cada pedra vem da posição fixa na grade ASCII. As 3 variantes diferem em 2–3 pedras claras e 1 rachadura | P0 |
| **Sarjeta** | tile 64 × 32 | faixa de 8 px `#3F3A3C` com 2 ralos `#1B1418`, ao longo do meio-fio | P0 |
| **Calçada elevada** | bloco 64 × 38 (6 px) | lajes de 16 × 8 px `#8A817A`/`#5E5755`; face do meio-fio `#3F3A3C`, topo realçado `#C9B58F`; mesma peça na calçada oposta | P0 |
| **Soleira** | bloco 64 × 36 | degrau de madeira `#7A4B2B` de 4 px diante da loja | P0 |
| **Lampião de rua** | 14 × 76 | poste de ferro `#3B3F4A` em "7", lanterna `#FFC77A`, base de pedra; 1 a cada 4 células (x = -2, 2, 6, 10, em y = -2 e y = -6); luz de 0,35 a 0,6 conforme a hora | P0 |
| **Poças** | 32 × 16, 2 variantes | losango achatado com reflexo `#BFD4FF` (alfa 55%) e 2 px de brilho que cintila (2 quadros, 1,2 s); 3 a 5 por trecho | P1 |
| **Carroça parada** | 112 × 56 (2 × 1) | madeira, 2 rodas (raios de 1 px), lona `#C9B58F`, sacos e barris | P1 |
| **Barris e caixotes** | 28 × 34 / 32 × 30 | reaproveitam o §7 | P0 |
| **Barraca do padeiro** | 64 × 64 | toldo mel `#C99A5B` e linho; balcão com **pães** de 6 px | P1 |
| **Barraca do verdureiro** | 64 × 64 | toldo verde-erva `#6F8A4A`; caixas de **repolhos** (`#97B068`) e cenouras (`#D8942E`) | P1 |
| **Barraca de tecidos** | 64 × 64 | toldo ameixa `#6B3A6E`; **rolos** de pano azul-real e verde | P1 |
| **Fachadas vizinhas** | blocos 64 × 72 (≤ 40 px de empena visível) | 3 casas por lado em perfil baixo: porta e janela, reboco `#C9B58F`/`#8F7E62` ou pedra, **sem telhado** (cutaway), 2 variantes por lado; **sem ferrugem** | P1 |
| **Vasos, plantas, banco** | 16–32 px | decoração de calçada | P2 |
| **Árvore pequena** (calçada oposta, 1) | 40 × 64 | copa `#6F8A4A`, tronco `#4B2E1B` | P2 |

**Para a rua "viver" sem poluir: 3 zonas de profundidade e densidade-alvo**

| Zona | Células | Densidade-alvo | Tratamento visual |
|---|---|---|---|
| **Z1 — Vitrine** | y = -1, -2 | **máx. 5 elementos** em 8 células (parapeito, placa, vaso, lampião, fila) | contraste e saturação totais; luz quente da loja; contorno de Tinta |
| **Z2 — Passagem** | y = -3 a -5 | estática baixa: **máx. 2 grandes** (carroça, barraca) e 4 pequenos (barris, poças) a cada 10 células; o resto é piso livre | saturação -10%; as pessoas são a "decoração" |
| **Z3 — Fundo** | y = -6 e vizinhos | barracas e fachadas contínuas | saturação -15%, contorno `#2A1D1A` (em vez de Tinta), contraste 10% menor; **altura ≤ 40 px** (exceto lampiões e a árvore), para nunca cobrir a rua ou a loja |

Orçamento por captura: **≤ 10 pedestres, ≤ 3 props grandes, ≤ 8 pequenos**; lampiões não contam; nenhum prop igual ao vizinho imediato.

**Geração:** `StreetArtGenerator.cs` (novo): `DrawCobble(variant)`, `DrawGutter()`, `DrawSidewalk()`, `DrawCurb()`, `DrawLamppost(lit)`, `DrawPuddle(frame)`. `StreetPropArtGenerator.cs` (novo): `DrawCart`, `DrawStall(kind)`, `DrawNeighbor(side, variant)`. Tiles criados como os atuais (`Tile` em `AnvilClickerPaths.Tiles`). **Esforço L; P0 para calçamento, calçada e lampião; P1 para o resto.**

### 15.3 Pedestres

**Corpos-base (3)** para poupar esforço: `adult` (48 px), `robe` (vestido/hábito, 48 px), `child` (36 px). Cada tipo = **corpo + paleta + acessório** (camadas carimbadas). Canvas 32 × 56 (cavaleiro até 54 px de altura), pivô nos pés `(0,5; 0,07)`, contorno de Tinta, sombra de contato.

| # | Tipo | Altura | Silhueta (leitura a 48 px) | Facção / cores | Acessório-chave |
|---|---|---|---|---|---|
| 1 | **Aldeão** | 46 | torso reto, chapéu de palha de aba curta | linho `#E0CFA8` e verde-erva `#6F8A4A` | enxada ou saco no ombro |
| 2 | **Aldeã com cesto** | 44 | vestido em trapézio, lenço branco | ocre `#B88A3E`, lenço `#F3E9D8` | **cesto** de 10 × 8 px no braço |
| 3 | **Criança** | 36 | baixa, cabeça grande, cabelo de espeto | tons vivos (azul-celeste, ocre) | pião ou pão; corre em zigue-zague |
| 4 | **Guarda** | 50 | capacete de nasal e **lança** vertical (24 px acima da cabeça) | azul-real `#2F4F8F` | lança, escudo pequeno |
| 5 | **Soldado** | 50 | cota de malha, **elmo redondo**, escudo grande | azul-real e ferro `#9AA3B2`, capa curta | escudo; marcha em dupla |
| 6 | **Mercador com carga** | 46 (30 px de largura) | **fardo enorme** nas costas, chapéu de aba larga | ameixa `#6B3A6E` | fardo `#C9B58F` com corda; anda curvado e devagar |
| 7 | **Monge** | 50 | **hábito** até o chão e capuz (silhueta de sino) | marrom `#6E4528`, corda de linho | rosário (1 px), mãos escondidas |
| 8 | **Cavaleiro** | 54 | armadura, **pluma** vermelha, capa que bate | aço e azul-real, pluma `#D8452F` | espada na cintura; ritmo majestoso |
| 9 | **Bardo** | 48 | **chapéu com pena** e **alaúde** nas costas, capa remendada | vinho `#8E3B4E` e ocre | alaúde de 10 × 14 px; toca nas pausas (2 quadros) |
| 10 | **Cão** | 16 × 22 | 4 patas, rabo erguido | pelagem `#A9713F` ou `#E2D3AE` | caminhada de 4 quadros, mais rápida; senta (1 quadro) |

**Quadros:** caminhada de **4 quadros por direção**, desenhada em **DownRight e UpRight** (DownLeft e UpLeft espelham): **8 quadros desenhados por corpo-base**, não por tipo. Os acessórios são **camadas** com 4 deslocamentos (±1 px de balanço). Cada tipo tem 1 quadro **parado** e, onde faz sentido, 1 quadro de **pausa** (olhar a vitrine: cabeça virada para a loja, 1 px mais baixa).

| Parâmetro | Valor |
|---|---|
| Velocidade (células/s) | criança 1,6 · adulto 1,1 · aldeã 1,0 · mercador 0,8 · monge 0,7 · cavaleiro 1,0 · guarda e soldado 0,9 · cão 1,8. A cadência do ciclo segue a regra do §5.7 (`velocidade_px / 24` ciclos/s) para os pés não deslizarem |
| Variação | cada instância sorteia (via `IRandom`, semente por spawn) velocidade ±15%, uma de 3 paletas de tecido e um atraso de início do ciclo (evita marcha sincronizada) |
| Pausas | 2–5 s diante da vitrine (x 1–5, y = -2), 3–6 s nas barracas, 1 s nas travessias; só **1 em cada 3** pedestres pára |

**Pedestre ambiente × cliente com encomenda**

| Aspecto | **Ambiente** | **Cliente (encomenda)** |
|---|---|---|
| Saturação da roupa | **-20%** (paleta "apagada") | paleta cheia |
| Contorno | Tinta, fixo | Tinta + **contorno de seleção** `#FFE08A` pulsante de 1 px (0,8 s) |
| Indicador acima da cabeça | nenhum | **balão de 28 × 24 px** com o **ícone da peça pedida** (16 × 16) e **moldura colorida pelo prazo** |
| Cor do prazo | | verde `#7FB05A` > 50% · ouro `#F2B632` 20–50% · vermelho `#D8452F` < 20% (pulsa a cada 1,2 s) |
| Chegada | passa direto | caminha **até a fila** (§15.4); ao chegar, o balão ganha "!" e o cliente dá um pulo de 2 px |
| Saída | segue seu caminho | **emote** de 1,5 s: coração `#D8452F` (satisfeito), nuvem `#8A817A` (insatisfeito), moeda (pagou) |
| Interação | não clicável | clicável: abre o cartão de encomenda (§10.4) |
| Reconhecimento | | faixa de facção de 1 px na gola |

**Geração:** `PedestrianArtGenerator.cs` (novo): `DrawBody(kind, dir, frame)`, `DrawAccessory(type, dir, frame)`, `DrawDog(frame)`, `MutePalette(palette, 0.8)`. Indicadores em `UiArtGenerator.DrawClientBubble` (§12, item 1.6). **Esforço L, P1** (3 corpos × 8 quadros + 10 acessórios). **Mínimo P0 da avenida:** 3 tipos (aldeão, aldeã, guarda).

### 15.4 Rotas e ritmo do dia

**Padrões de movimento** ("ida" = sentido +x, cima-direita na tela; "volta" = -x, baixo-esquerda):

| Rota | Pista | Comportamento |
|---|---|---|
| **Ida e volta** pela calçada | y = -2 | sentido único por pessoa, cruza o enquadramento em 15–25 s; pára na vitrine (x 1–5) |
| **Pista central** | y = -4 | carroças, cavaleiros, soldados; evita a calçada |
| **Pista de volta** | y = -5 | sentido contrário (-x), ritmo mais lento |
| **Travessia** | em x = -2, 4, 10 | de y = -2 a y = -6 (e o inverso), 4 a 5 células; pára 1 s no meio e olha para os lados (2 quadros) |
| **Barracas** | y = -6 | aldeãs e mercadores param 3–6 s |
| **Fila do balcão** | (6,-2), (5,-2), (4,-2) | o cliente deixa a pista y = -3, sobe o meio-fio e entra na fila; **máx. 3** na fila; os extras esperam em y = -3 olhando a loja |
| **Saída** | | ao ser atendido, "emote" e volta à pista |

Spawn e despawn **fora da tela**, em x = -6 e x = 14; nada nasce no meio da rua.

**Densidade por hora do dia** (ver pergunta 2):

| Faixa | Luz ambiente | Pedestres na tela | Clientes (máx.) | Detalhes |
|---|---|---|---|---|
| **Manhã** 06–11 h | `#6A7AA8`, 0,60 | 6–8 | 2 | padeiro com pães quentes (vapor), aldeãs, crianças |
| **Tarde** 11–18 h | `#8A7A66`, 0,55 | 8–10 (pico) | 3 | barracas cheias, soldado e cavaleiro |
| **Fim de tarde** 18–20 h | `#7A5A55`, 0,45 | 4–6 | 2 | luz dourada na fachada; lampiões **acendem** (fade de 20 s) |
| **Noite** 20–06 h | `#1E2442`, **0,30** | **2–3** (guarda com lanterna, bardo, cão) | 1 | **lampiões acesos** (0,6; raio 2,2); **janela e lanterna** da loja em `#FFC77A`, com poça de luz no calçamento; barracas fechadas (toldos recolhidos, caixas empilhadas); plaquinha "FECHADO" se não há encomendas; a **brasa da forja** é a luz dominante |

A troca de faixa tem fade de 20 s no ambiente e nos lampiões; nunca corte seco.

### 15.5 Prosperidade (3 estágios): fachada e rua

| Elemento | **Nível 0 — Humilde** | **Nível 1 — Estabelecida** | **Nível 2 — Próspera** |
|---|---|---|---|
| Toldo | desbotado (cores -25%), **remendado** (2 quadrados de pano), franja rasgada | **novo**, listras ferrugem e linho | listras **ferrugem e ouro**, franjas de ouro |
| Placa | madeira crua, 1 corrente, bigorna riscada a carvão | madeira pintada, bigorna dourada | **ferro forjado** com moldura de ouro e lanterna |
| Parapeito | 1 arma à mostra | 3 armas | 4 armas e **mostruário de vidro** (brilho `#BFD4FF`) |
| Fachada | reboco manchado, 1 pilar rachado | pilares inteiros | base de pedra entalhada, bandeirola do reino |
| Vasos e plantas | nenhum | 2 vasos na soleira | floreiras e 4 vasos |
| Calçada da loja | terra e lajes quebradas, 2 poças | lajes inteiras | lajes polidas e **tapete** na soleira |
| Mobiliário de rua | 1 lampião | 2 lampiões, 1 banco | 3 lampiões de latão, bancos, 1 árvore |
| **Movimento** | 3–4 pedestres, só aldeões | 6–8, com guarda, mercador, bardo | **8–10**, com cavaleiro, soldados, carroça de entrega |
| Rua | calçamento com buracos, 5 poças | limpo, 3 poças | limpo, bandeirolas entre os lampiões |

A fachada e a rua mudam por troca de módulos e props em slots; a malha não é regenerada. **Esforço M (variantes sobre os módulos de 15.1 e 15.2), P1.**

### 15.6 Câmera

Contas a 1280 × 720 px de tela. A 2× a janela de mundo é **640 × 360 px**; a 1×, **1280 × 720 px**.

| Zoom | Janela de mundo | O que mostra | Uso |
|---|---|---|---|
| **3×** | 427 × 240 | bigorna, ferreiro, forja | modo forja |
| **2×** (padrão) | 640 × 360 | com a câmera em torno de (4, -1): a **fachada inteira**, as **5 linhas da avenida** em ~12 células de comprimento (x ≈ -3 a 9) e **~116 px de mundo (232 px de tela)** abaixo do canto frontal (0,0); o topo da Oficina (cantos de fundo) é cortado em ~68 px de mundo | exploração normal; a câmera segue o ferreiro com **viés de -24 px** (mundo) em direção à rua enquanto ele está nas 3 linhas da frente |
| **1×** (vista geral) | 1280 × 720 | a loja inteira e **toda a avenida** (x -6 a 14, ~930 × 500 px) | tecla de "vista da rua" (por exemplo Tab) e roda do mouse; é a **resolução nativa da cena** |

- **Sim, a câmera deve poder se afastar para 1×** para o jogador ver a rua inteira. O zoom é **só inteiro** (1×, 2×, 3×): a roda do mouse "encaixa" com transição de 0,25 s. A 1× o ferreiro tem 48 px (6,7% da altura da tela), ainda legível porque a silhueta foi feita para 24 px (§5.1).
- **Faixa de rua a 2×:** abaixo do canto frontal aparecem 116 px de mundo, suficientes para y = -2 a y = -6 junto ao balcão. Os trechos x > 10 e x < -3 só aparecem a 1× ou quando a câmera anda.
- Limites da câmera (Presentation): caixa da Oficina e da avenida (x -6 a 14, y -6 a 7), com margem de 1 célula.

### 15.7 Assets e critérios de aceitação da avenida

| # | Asset | Como gerar | Esf. | Prio. |
|---|---|---|---|---|
| A.1 | **Fachada:** pilares, 3 parapeitos, meia-porta, janela de atendimento, viga | `FacadeArtGenerator.Generate()` | L | **P0** |
| A.2 | **Toldo** e **placa** (3 níveis; placa com 2 quadros de balanço) | `FacadeArtGenerator.DrawAwning` e `DrawHangingSign` | M | **P0** (nível 0), P1 (níveis 1 e 2) |
| A.3 | **Calçamento** (3 variantes), sarjeta, calçada elevada, soleira, meio-fio | `StreetArtGenerator` | M | **P0** |
| A.4 | **Lampião de rua** (apagado e aceso) | `StreetArtGenerator.DrawLamppost` | S | **P0** |
| A.5 | **Muro lateral baixo** (x = -1) com colunas e trepadeira | `FacadeArtGenerator.DrawSideWall` | M | P1 |
| A.6 | **Pedestres:** 3 corpos-base e 3 tipos (aldeão, aldeã, guarda) | `PedestrianArtGenerator` | L | P1 |
| A.7 | **Indicadores de cliente:** balão, moldura de prazo, ícones de arma, emotes | `UiArtGenerator.DrawClientBubble` | M | P1 |
| A.8 | Demais pedestres (criança, soldado, mercador, monge, cavaleiro, bardo, cão) | `PedestrianArtGenerator` | L | P1/P2 |
| A.9 | **Barracas** (3) e **carroça** | `StreetPropArtGenerator` | L | P1 |
| A.10 | **Fachadas vizinhas** (3 por lado) | `StreetPropArtGenerator.DrawNeighbor` | M | P1 |
| A.11 | **Poças** e **poças de luz** (loja, janela, lampião) | `StreetArtGenerator`, `FxArtGenerator` | S | P1 |
| A.12 | **Variantes de prosperidade** dos módulos | `_tier1` e `_tier2` | M | P1 |
| A.13 | **Hora do dia:** ambientes, lampiões, "FECHADO" | parâmetros em ScriptableObject (Presentation) | M | P2 |
| A.14 | Vasos, banco, árvore, bandeirolas, tapete da soleira | `StreetPropArtGenerator` | M | P2 |

Todos seguem as convenções do §12 (determinísticos, idempotentes, só cores da paleta do §4, mais `#BFD4FF` e `#FFC77A`, já usadas nas luzes).

**Critérios de aceitação** (captura de **1280 × 720**, zoom 2×, ferreiro na posição inicial, estágio 0, tarde; e outra a **1×**):

- [ ] **Em 1 s** se lê "loja aberta": toldo ferrugem e linho, placa com bigorna e luz quente saindo para a calçada.
- [ ] O toldo ferrugem é a **única** faixa dessa cor na avenida; as barracas usam mel, verde-erva e ameixa.
- [ ] A fachada **não esconde** o interior: ferreiro, bigorna e balcão continuam visíveis; nada no parapeito passa de 18 px.
- [ ] Há **soleira, calçada elevada (6 px), sarjeta e calçamento** com 3 variantes, sem ruído de pixel.
- [ ] **De 5 a 10 pedestres** na tela, com ≥ 4 silhuetas diferentes e ≥ 1 em pausa diante da vitrine; nenhum sobreposto a ponto de impedir a leitura.
- [ ] O cliente na fila tem **balão com o ícone da peça e moldura colorida pelo prazo**; nenhum pedestre ambiente tem indicador.
- [ ] **3 zonas legíveis:** Z1 é a mais saturada e contrastada; Z3 é visivelmente mais calma e nada nela passa de 40 px (exceto lampiões).
- [ ] **≤ 3 props grandes e ≤ 8 pequenos** de rua; um lampião a cada ~4 células.
- [ ] **A 1×** aparecem a loja inteira e toda a avenida (x -6 a 14), e o ferreiro ainda é identificável pelo lenço vermelho.
- [ ] **A 2×** aparecem a fachada completa e as 5 linhas da avenida junto ao balcão, com zoom inteiro (pixels do mesmo tamanho).
- [ ] **Noite:** lampiões acesos, 2–3 pedestres, janela e lanterna da loja em luz quente, e a forja ainda é o ponto mais brilhante.
- [ ] **Pipeline:** `RunAllBatch` duas vezes seguidas não altera nenhum PNG.

### 15.8 Perguntas em aberto (avenida)

1. **Layout:** posso pedir ao agente principal que mova o **balcão** de (6,2) para **(6,0)**, encostado na fachada, com o cliente em (6,-2) e a fila em (5,-2) e (4,-2)? Sem isso o balcão não "atende a rua".
2. **Relógio do dia:** a hora (manhã, tarde, noite) segue o **relógio de jogo** (ciclo de poucos minutos) ou o **relógio real** do computador? Muda a quantidade de variantes de luz e a regra do "FECHADO".
3. **Pedestres:** são **só cenário** ou o jogador pode clicar em alguns (guarda, mercador) para ouvir uma fala ou receber uma dica de missão? Isso decide quantas fichas de personalidade e retratos (P2) vamos gastar.

### 15.9 Respostas às perguntas da seção 15 (2026-10-06)

1. **Balcão:** aprovado mover para **(6, 0)** (colado à fachada), com o cliente atendido em (6, -2) e a fila em (5, -2) e (4, -2). A mudança entra em `WorldDataSeeds` e na `RoomDefinition` da Oficina durante o M3.5.
2. **Hora do dia:** segue o **relógio do jogo** (`DayClock`), não o relógio real. Até o `DayClock` existir (M4), o M3.5 usa um ciclo simples de poucos minutos só para a luz e a densidade de pedestres.
3. **Pedestres:** **só cenário** no MVP. Falas e dicas de missão por clique podem entrar depois, junto com os clientes recorrentes (M6).
