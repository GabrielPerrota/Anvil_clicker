# Anvil Clicker — Roadmap

> **Status:** v1, aprovada em 2026-10-03.
> Cada milestone vira uma Milestone no GitHub. Cada item `- [ ]` vira uma issue.

## Mudanças em relação ao roadmap original

| Mudança | Motivo |
|---|---|
| **Save básico + progresso offline foram para o M2** | Um jogo idle sem save é impossível de testar por mais de uma sessão. O offline é uma função pura, barata, e entra junto com o save |
| **Capítulos (crises) vieram para o M5; runas foram para o M6** | O MVP aprovado inclui o capítulo 1, e a crise dá propósito ao loop antes da magia |
| **M7 ficou com conquistas, eventos e capítulos 2–4** | Conteúdo que depende de runas (cap. 3 e 4) |
| **MVP = tag `v0.1.0` ao fim do M5**, com build WebGL para playtest | Escopo aprovado: M1–M4 + capítulo 1 |

## Fluxo de branches

- Para cada milestone: `develop` → `feature/<nome>` → PR para `develop` (squash ou merge com Conventional Commits).
- Ao fim de milestones-chave: `develop` → `main`, com tag.
- Versões: `v0.0.x` durante o M1–M4, **`v0.1.0` = MVP**, `v0.x` nas milestones seguintes e `v1.0.0` no fim do M8.

---

## M0 — Planejamento ✅
- [x] Ler o repositório e levantar o estado
- [x] Decisões: visual, plataforma, monetização, MVP
- [x] Aprovar GDD, ARCHITECTURE e ROADMAP

## M1 — Fundação e o primeiro golpe
**Meta:** abrir o projeto, clicar na bigorna e ver o ouro subir.
- [ ] `chore`: `.gitignore` Unity, `.gitattributes` (LFS + text), Force Text, README, CHANGELOG, CLAUDE.md
- [ ] `chore`: projeto Unity 6 LTS + URP 2D Renderer + Input System + Test Framework
- [ ] `feat(editor)`: `Setup/Create Folders`, `Setup/Configure Project`, asmdefs
- [ ] `feat(core)`: `GameState`, `Wallet`, `ForgeService` (golpe, crítico, conclusão), `IClock`/`IRandom`
- [ ] `feat(core)`: `NumberFormatter` (K, M, B...)
- [ ] `feat(data)`: SOs mínimos (arma, balanceamento, database) para não ter números mágicos no código
- [ ] `feat(runtime)`: `GameBootstrap` (composition root) + input da bigorna. O `GameLoop` (tick) vai para o M2, junto com o idle
- [ ] `feat(ui)`: HUD em UI Toolkit (ouro, barra de progresso)
- [ ] `feat(fx)`: juice v1 (faíscas, número flutuante, squash, shake no crítico)
- [ ] `feat(editor)`: placeholder sprites + `Scenes/Build Workshop Scene` (versão mínima: bigorna + câmera)
- [ ] `test`: Forge, Wallet, NumberFormatter

**Pronto quando:** dá Play, cada click forja uma adaga de ferro, ela é vendida automaticamente e o ouro aparece formatado. Os testes passam.
No M1 a venda é automática e só existe uma arma, porque balcão e catálogo vêm depois.

## M2 — Economia, idle e persistência
**Meta:** o loop incremental de verdade, que sobrevive a fechar o jogo.
- [ ] `feat(core)`: `EconomyFormulas` (custo geométrico, lote, máx)
- [ ] `feat(core)`: `ModifierStack` + `UpgradeService` (martelos, forja, precisão)
- [ ] `feat(core)`: `WorkforceService` (aprendizes, PF/s, marcos)
- [ ] `feat(data)`: SOs `UpgradeDefinition`, `ApprenticeDefinition`, `GameBalanceConfig`, `GameDatabase` + `Data/Generate Sample Data`
- [ ] `feat(ui)`: painel de upgrades e aprendizes (×1/×10/×100/Máx)
- [ ] `feat(save)`: SaveSystem JSON versionado, escrita atômica, `.bak`, autosave
- [ ] `feat(core)`: `OfflineProgressCalculator` + modal de resumo
- [ ] `feat(editor)`: `Data/Validate Database`, `Save/*`, `Debug/Add Gold`
- [ ] `test`: fórmulas, modificadores, save/migração, offline
- [ ] `chore`: primeiro build WebGL (validar o save no IndexedDB)

**Pronto quando:** dá para jogar 30 min, fechar, voltar e ver o resumo offline correto.

## M3 — A loja isométrica
**Meta:** andar pela ferraria e interagir com as estações.
- [ ] `feat(world)`: Grid isométrico, tilemaps Floor e Walls, sorting por eixo Y
- [ ] `feat(world)`: player com WASD relativo à tela + animação em 4 direções (placeholder)
- [ ] `feat(world)`: `Interactable` + estações (Bigorna, Forja, Balcão, Mensageiro) com prompt "E"
- [ ] `feat(world)`: modo forja na bigorna (zoom de câmera, golpes)
- [ ] `feat(core)`: `IsoGridPathfinder` (A*) + click na estação → caminhar e interagir
- [ ] `feat(camera)`: follow, limites, zoom
- [ ] `feat(world)`: `RoomService` + `RoomBuilder`: comprar o Depósito faz a sala aparecer com animação
- [ ] `feat(fx)`: Light2D da forja (pulsante, mais forte com o upgrade de calor)
- [ ] `feat(editor)`: `Build Workshop Scene` completo e idempotente
- [ ] `test`: pathfinder, regras de desbloqueio de sala

**Pronto quando:** você anda, abre cada painel na estação certa e compra uma sala que surge no mapa.

## M4 — Materiais e catálogo de armas
**Meta:** variedade e decisões de produção.
- [ ] `feat(data)`: `WeaponTypeDefinition` (6 tipos), `MaterialDefinition` (ferro, aço)
- [ ] `feat(core)`: receita ativa (tipo × material), custo de material, valor e PF calculados
- [ ] `feat(core)`: `InventoryService` + `SalesService` (balcão manual, upgrade Vendedor, reservas)
- [ ] `feat(ui)`: seletor de receita na bigorna, prateleira, painel do balcão
- [ ] `feat(world)`: armas prontas aparecem na prateleira (sprites placeholder)
- [ ] `test`: valor/PF por receita, venda, reserva, offline com custo de material

**Pronto quando:** escolher aço em vez de ferro é uma decisão real, visível no ouro/min.

## M5 — Crise 1: Bandidos na Estrada 🎯 **MVP v0.1.0**
**Meta:** o primeiro capítulo completo, do começo ao fim.
- [ ] `feat(data)`: `ChapterDefinition` (carta, metas, recompensas)
- [ ] `feat(core)`: `ChapterService` (entregas, progresso, conclusão, desbloqueios, reputação)
- [ ] `feat(ui)`: carta do rei (modal), painel do Mensageiro, rastreador de metas no HUD
- [ ] `feat(content)`: capítulo 1 + balanceamento do MVP (meta: ~45–60 min até concluir)
- [ ] `test`: metas, entrega, recompensa, save do capítulo
- [ ] `chore`: build WebGL de playtest + release `v0.1.0`

**Pronto quando:** um jogador novo conclui o capítulo 1 sem ajuda, em cerca de 1 h.

## M6 — Runas e magia
- [ ] Essência Arcana (fonte e consumo)
- [ ] Bancada de runas (sala) + gravar runa na receita
- [ ] Runas base (Fogo, Gelo, Sagrado, Raio, Sombra) + tags nas armas
- [ ] Combinação de runas + descoberta de receitas
- [ ] Itens mágicos raros
- [ ] Mithril (tier 3)
- [ ] Testes de multiplicadores e combinações

## M7 — Conteúdo, eventos e conquistas
- [ ] Capítulos 2 (Orc), 3 (Mortos-vivos) e 4 (Dragão) + materiais arcanos
- [ ] Faísca dourada, Mercador viajante, Encomenda urgente
- [ ] Conquistas (SO) + painel + bônus
- [ ] Upgrades de offline (teto/eficiência) e do Sino do Capataz
- [ ] Testes de eventos (com `IRandom`/`IClock`) e conquistas

## M8 — Legado, balanceamento e polimento → **v1.0.0**
- [ ] Legado: cálculo de marcas, reset, bônus permanentes, Salão do Legado
- [ ] Árvore de talentos do Legado (se aprovada)
- [ ] Balanceamento com planilha/simulação (Editor tool que simula N horas)
- [ ] Juice v2: VFX, áudio, trilha, transições
- [ ] Configurações (volume, shake, notação, idioma), acessibilidade
- [ ] Build PC final + WebGL

---

## Riscos conhecidos

| Risco | Mitigação |
|---|---|
| Não opero o Editor | Editor scripts geradores e idempotentes, e passos manuais numerados ao fim de cada tarefa |
| Sorting isométrico 2D (personagem atrás e na frente de objetos) | Custom Axis + pivô nos pés + `SortingGroup`, validado cedo no M3 |
| Persistência no WebGL | Teste de build já no M2 |
| Balanceamento de incremental | Valores em SOs + tool de simulação no M8 + playtests a cada milestone |
| Arte placeholder por muito tempo | Pipeline de sprites pensado para trocar o PNG sem mexer em prefabs |
