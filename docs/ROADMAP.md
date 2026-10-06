# Anvil Clicker — Roadmap

> **Status:** v2, **aprovada em 2026-10-06**. Acompanha o [GDD v2](GDD.md): o jogo agora gira em torno de **encomendas, missões do reino e uma economia pequena**.
> Cada milestone vira uma Milestone no GitHub. Cada item `- [ ]` vira uma issue.

## O que mudou em relação à v1

| Mudança | Motivo |
|---|---|
| M4–M8 foram redesenhados em torno de encomendas, qualidade e missões | O jogo deixou de ser um clicker de números enormes |
| Novo **M3.5 Identidade visual** antes do M4 | A loja precisa parecer uma oficina medieval de verdade, com um ferreiro marcante |
| Runas e magia saíram do caminho do MVP | Entram depois, quando o loop de encomendas estiver sólido |
| `Legado` (prestígio com bônus enormes) foi removido do roadmap | Não combina com a economia pequena. Pode voltar como algo narrativo |
| **MVP = `v0.1.0` ao fim do M5** | Visual novo + encomendas + Capítulo 1 jogável |

## Fluxo de branches

- Para cada milestone: `develop` → `feature/<nome>` → merge `--no-ff` em `develop`.
- Ao fim de cada milestone estável: `develop` → `main`, com tag.
- Versões: `v0.0.x` até o MVP, **`v0.1.0` = MVP**, `v0.x` depois e `v1.0.0` no fim do M8.

---

## M0 — Planejamento ✅
## M1 — Fundação e o primeiro golpe ✅ (`v0.0.1`, `v0.0.2`)
Projeto Unity, repositório, click na bigorna, HUD, juice, testes.

## M2 — Economia, idle e persistência ✅ (`v0.0.3`)
Fórmulas de custo, modificadores, melhorias, aprendizes, save JSON versionado, offline, loja. *Boa parte da infraestrutura (carteira, modificadores, save, validação de dados) é reaproveitada na v2.*

## M3 — A loja isométrica ✅ (`v0.0.4`)
**Meta:** andar pela ferraria e usar as estações.
- [x] Grade isométrica, paredes com colisão, ordenação por Y
- [x] Ferreiro com WASD relativo à tela e 4 direções
- [x] Estações com prompt "E": Bigorna, Forja, Mesa de Melhorias, Balcão, Mensageiro, Depósito
- [x] Modo forja na bigorna (zoom e golpes só ali)
- [x] A* + clique na estação para andar até ela
- [x] Câmera com follow, limites e zoom
- [x] Salas compráveis que surgem no mapa com animação
- [x] Luz da forja que reage às melhorias
- [x] `-saveDir` para rodar sem tocar no save real
- [x] Build WebGL e persistência do save no navegador (IndexedDB via `FS.syncfs`)

## M3.5 — Identidade visual
**Meta:** a oficina parece uma pequena loja medieval aconchegante, com um ferreiro inconfundível, e os números do jogo atual ficam pequenos.
- [ ] `docs/ART_DIRECTION.md` (guia de estilo) escrito pelo agente de direção de arte
- [ ] **Novo ferreiro:** sprite maior, 4 direções, caminhada de 4 quadros, martelada, parado
- [ ] **Fachada aberta e avenida:** vitrine com toldo e placa, calçamento, lampiões, barracas ao fundo, **pedestres** ambiente andando pela rua (ida e volta, com pausas)
- [ ] **Cenário:** paredes de reboco com vigas e janela, piso de tábuas e pedra, ferramentas penduradas, barris, caixotes, prateleira, balança
- [ ] Forja, bigorna, balcão e mesas redesenhados; sombras de contato
- [ ] Suporte a props decorativos (sem colisão) em `RoomDefinition`/`RoomBuilder` e a animação do `PlayerController`
- [ ] **Rebalanceamento de transição:** melhorias com nível máximo baixo e custo suave, no máximo 3 aprendizes, preços e metas pequenos
- [ ] Capturas antes e depois; testes verdes

**Pronto quando:** uma captura de 1280 × 720 passa no checklist do guia de arte e o jogo atual já parece "pequeno".

## M4 — Encomendas e clientes
**Meta:** o loop novo: encomenda → forja com qualidade → entrega → pagamento → contas do dia.
- [ ] `DayClock`: ciclo de dia (3–4 min), resumo e contas (aluguel, salários)
- [ ] **Clientes chegam pela avenida**, entram na fila do balcão (indicador da peça e do prazo) e só então a encomenda entra no quadro
- [ ] Clientes e **quadro de encomendas** (gerador por perfil, prazo, preço, qualidade mínima, gorjeta)
- [ ] **Forja com qualidade:** anel de ritmo, calor, foles, resultado ★1–5 (Core testável)
- [ ] Peças e materiais (ferro e aço); estoque; compra de material no Depósito
- [ ] **Inventário e balcão:** guardar peças, entregar encomendas
- [ ] Reputação por facção (base)
- [ ] Migração do save (v1 → v2) e adaptação de `Workforce`/`Offline` ao novo modelo
- [ ] Testes: qualidade, preço, geração de encomendas (com `IRandom`), contas, save/migração

**Pronto quando:** dá para jogar 3 dias seguidos atendendo clientes e fechando as contas.

## M5 — Missões do reino · MVP `v0.1.0`
**Meta:** o Capítulo 1 completo, do começo ao fim.
- [ ] `ChapterService` e `QuestService`: metas, prazos, entrega, falha e prorrogação
- [ ] Cartas do rei (UI), rastreador de metas e mensageiro funcional
- [ ] Capítulo 1 (Bandidos na Estrada) com recompensas: aço, Depósito, 1º aprendiz
- [ ] Balanceamento do MVP (~45 min até concluir o capítulo)
- [ ] Build WebGL de playtest e release `v0.1.0`

**Pronto quando:** um jogador novo conclui o Capítulo 1 sem ajuda.

## M6 — Oficina viva
- [ ] 3 aprendizes com nome, talento e salário
- [ ] Todas as melhorias (~12) e a loja que melhora visualmente (3 níveis)
- [ ] Quarto dos aprendizes; clientes recorrentes; reputação completa

## M7 — Capítulos 2–4 e eventos
- [ ] Capítulos 2 (Cerco Orc), 3 (Praga) e 4 (Dragão), mithril
- [ ] Eventos (mercador viajante, encomenda urgente, festival) e conquistas leves

## M8 — Polimento · `v1.0.0`
- [ ] Áudio e trilha, transições, acessibilidade (forja tranquila, shake), idiomas
- [ ] Balanceamento por simulação, builds finais PC e WebGL
- [ ] Pós-MVP opcional: runas e magia, legado narrativo

---

## Riscos conhecidos

| Risco | Mitigação |
|---|---|
| Pixel art por código tem teto de qualidade | Guia de arte, iteração com capturas; saída: pack de assets escolhido pelo usuário |
| Pivô de design mexe em sistemas prontos | Reaproveitar `Wallet`, `ModifierStack`, save e validador; migração de save com `ISaveMigration` |
| Ritmo da forja pode cansar | Opção "forja tranquila" e ajuste de dificuldade; playtest cedo no M4 |
| Persistência no WebGL | Teste no navegador antes do M4 |
| Ordenação isométrica (personagem × paredes) | Eixo Y customizado, pivôs nos pés, validação por captura |
