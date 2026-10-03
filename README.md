# Anvil Clicker

Clicker incremental medieval em 2D isométrico. Você é o ferreiro de um reino em crise: forja armas para os soldados enfrentarem bandidos, orcs, mortos-vivos e um dragão. A ferraria cresce com novas estações, materiais melhores, aprendizes e, por fim, runas e magia.

- **Engine:** Unity **6000.3.25f1** (Unity 6.3 LTS), URP 2D Renderer, Input System, UI Toolkit
- **Plataformas:** PC (Windows) e WebGL (playtest)
- **Status:** em desenvolvimento. Veja o [ROADMAP](docs/ROADMAP.md) e o [CHANGELOG](CHANGELOG.md)

## Documentação

| Documento | Conteúdo |
|---|---|
| [docs/GDD.md](docs/GDD.md) | Design do jogo: loops, recursos, capítulos, balanceamento |
| [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md) | Camadas, assemblies, serviços, dados, save, testes |
| [docs/ROADMAP.md](docs/ROADMAP.md) | Milestones M1–M8 e issues |
| [CLAUDE.md](CLAUDE.md) | Convenções do projeto (para pessoas e para o Claude Code) |

## Como abrir

1. Instale o **Git LFS** antes de clonar, porque os binários (png, wav, fbx…) ficam no LFS:
   ```bash
   git lfs install
   ```
2. Clone o repositório:
   ```bash
   git clone https://github.com/GabrielPerrota/Anvil_clicker.git
   ```
3. Instale a Unity **6000.3.25f1** com o módulo **Web Build Support**.
4. Abra a pasta do repositório pelo Unity Hub (*Add project from disk*), ou pelo Unity CLI:
   ```bash
   unity open .
   ```
5. Abra a cena `Assets/_Project/Scenes/Workshop.unity` e dê **Play**.

## Gerar conteúdo e configurar o projeto

Cenas, dados de exemplo e sprites placeholder são gerados por Editor scripts, no menu **Tools → Anvil Clicker**:

| Menu | O que faz |
|---|---|
| `Setup/Run All` | Executa todos os passos abaixo, em ordem |
| `Setup/Create Folders` | Cria a estrutura `Assets/_Project/...` |
| `Setup/Configure Project` | Sorting layers, eixo de ordenação 2D, input, player settings |
| `Art/Generate Placeholder Sprites` | Gera os PNGs placeholder |
| `Data/Generate Sample Data` | Cria os ScriptableObjects de exemplo |
| `Scenes/Build Workshop Scene` | Monta a cena `Workshop` |

Todos são idempotentes: rodar de novo atualiza em vez de duplicar. Também dá para rodar em batch, com o Editor fechado:

```bash
unity run . -- -executeMethod AnvilClicker.Editor.AnvilClickerSetup.RunAllBatch -logFile Logs/setup.log
```

## Testes

Os testes EditMode (Unity Test Framework) cobrem as fórmulas e os serviços do Core. Para rodar pelo Editor: *Window → General → Test Runner → EditMode*. Pelo CLI, com o Editor fechado:

```bash
unity test . --mode EditMode --output TestResults/editmode.xml
```

## Fluxo de trabalho

- **Branches:** `main` (estável), `develop` (integração), `feature/<nome>`.
- **Commits:** [Conventional Commits](https://www.conventionalcommits.org/) (`feat:`, `fix:`, `refactor:`, `docs:`, `test:`, `chore:`).
- **Merge opcional de cenas e prefabs** com o UnityYAMLMerge:
  ```bash
  git config merge.unityyamlmerge.driver '"H:/6000.3.25f1/Editor/Data/Tools/UnityYAMLMerge.exe" merge -p %O %B %A %A'
  ```
