# CLAUDE.md — Convenções do Anvil Clicker

Este guia vale para sessões futuras do Claude Code e para qualquer pessoa no projeto.

## Idioma
- Conversas, documentação e textos do jogo: **português do Brasil**.
- Código, nomes de classes, arquivos, branches e mensagens de commit: **inglês**.

## Projeto
- Unity **6000.3.25f1** (6.3 LTS), instalada em `H:\6000.3.25f1`. O projeto Unity fica na raiz do repositório.
- URP com **2D Renderer**, **Tilemap isométrico**, **Input System** (project-wide actions em `Assets/_Project/Settings/Input/AnvilControls.inputactions`) e **UI Toolkit** (UXML/USS).
- Plataformas: PC (Windows) e WebGL. Sem monetização.
- Documentos de referência: `docs/GDD.md`, `docs/ARCHITECTURE.md`, `docs/ROADMAP.md`. Leia antes de mudar sistemas.

## Arquitetura (regras)
- **Assemblies** em `Assets/_Project/Scripts/`:
  - `Core`: C# puro, `noEngineReferences: true`. Contém estado, serviços, fórmulas e formatação.
  - `Data`: ScriptableObjects que implementam as interfaces do Core.
  - `Runtime`: bootstrap, input, mundo e I/O.
  - `Presentation`: UI, VFX, áudio e câmera.
  - `Editor`: geradores e ferramentas.
  - Testes ficam em `Assets/_Project/Tests/EditMode`.
- **Dependências permitidas:** Presentation → Runtime → Data → Core. Presentation também → Core. O Core não referencia nada.
- **Toda mudança de estado passa por um serviço do Core.** UI e VFX só leem o estado e reagem a C# events.
- **Sem singletons nem barramento estático.** O `GameBootstrap` é o composition root e injeta o `GameContext` via `IGameContextConsumer.Bind`.
- Aleatoriedade e tempo entram por `IRandom` e `IClock`, para serem testáveis.
- **Números:** `double` + `NumberFormatter`. Nunca formatar números do jogo com `ToString()` direto na UI.
- **Dados:** todo SO tem um `id` string estável, que vai para o save. **Nunca renomear ids**; deprecie e crie outro.
- **Sem números mágicos de balanceamento no código.** Eles vão para ScriptableObjects.

## Unity sem abrir o Editor
- Tudo que exige o Editor vira um script em `Tools/Anvil Clicker/...`, **idempotente**, com entrada batch em `AnvilClicker.Editor.AnvilClickerSetup.RunAllBatch`.
- Se uma tarefa exigir passo manual no Editor, liste as instruções numeradas ao final.
- **Não editar `.unity`/`.prefab`/`.asset` à mão.** Gere pelos Editor scripts.
- A cena `Workshop` é gerada por `WorkshopSceneBuilder`. O `RunAllBatch` só a cria se ela não existir. Depois de mudar o builder, reconstrua com `-executeMethod AnvilClicker.Editor.AnvilClickerSetup.RebuildWorkshopSceneBatch` e commite a cena.
- O builder carrega os assets **depois** do `NewScene`: o modo Single descarrega assets e quebraria as referências. Mantenha essa ordem.
- Comandos (com o Editor **fechado** para este projeto):
  ```bash
  # gerar/atualizar conteúdo + checar compilação
  unity run . --timeout 900 -- -executeMethod AnvilClicker.Editor.AnvilClickerSetup.RunAllBatch -logFile Logs/setup.log
  # testes EditMode
  unity test . --mode EditMode --output TestResults/editmode.xml --timeout 900
  ```
- Erros de compilação: procure `error CS` em `Logs/setup.log`.

## Git
- Branches: `main` (estável, com tags), `develop` (integração), `feature/<nome>` (a partir de `develop`).
- Conventional Commits, pequenos: `feat(core):`, `fix(ui):`, `test(core):`, `docs:`, `chore:`, `refactor:`.
- Merge de feature em develop com `--no-ff`. Ao fim de uma milestone estável: develop → main + tag `vX.Y.Z`.
- Binários vão para o Git LFS (ver `.gitattributes`). Nunca commitar `Library/`, `Temp/`, `Logs/`, `UserSettings/`.
- Atualize o `CHANGELOG.md` a cada milestone.

## Forma de trabalho por milestone
1. Plan mode para planejar.
2. Pequenos incrementos.
3. Rodar setup + testes pelo CLI.
4. Commit.
5. Resumo: o que mudou, passos manuais no Editor e como testar.

**Não adicionar assets pagos nem dependências externas (incluindo pacotes fora dos oficiais já aprovados) sem perguntar.**
Pacotes aprovados: URP, Input System, 2D Sprite/Tilemap/Tilemap Extras, Test Framework, uGUI, Newtonsoft Json (M2+).
