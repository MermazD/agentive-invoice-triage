# ADR - Invoice extraction

Ces ADR reconstituent les décisions visibles dans le code et la documentation disponibles. Ils ne remplacent pas un historique de décisions source : les motivations passées sont indiquées comme des hypothèses lorsqu'elles ne sont pas explicitement documentées.

| ADR                                    | Décision                                                                   | Statut de la reconstitution                |
| -------------------------------------- | -------------------------------------------------------------------------- | ------------------------------------------ |
| [0001](0001-tesseract-cli.md)          | Utiliser Tesseract OCR via son exécutable CLI                              | Observée; motivations déduites             |
| [0002](0002-pdf-rasterization.md)      | Rasteriser les PDF, traiter les pages séquentiellement et agréger le texte | Observée; motivations déduites             |
| [0003](0003-semantic-kernel-plugin.md) | Exposer l'extraction comme plugin Semantic Kernel retournant du texte brut | Observée; motivations déduites             |
| [0004](0004-error-handling.md)         | Journaliser les erreurs d'extraction et retourner une chaîne vide          | Observée; politique opérationnelle déduite |
| [0005](0005-no-persistent-storage.md)  | Ne pas persister les documents ou les résultats dans ce module             | Absence observée; intention déduite        |
