#
# Translation
#--------------------------------------

.PHONY: l10n-push
l10n-push:
	tx push -s --skip

.PHONY: l10n-pull
l10n-pull:
	tx pull -a --skip --minimum-perc=75

.PHONY: l10n-clean
l10n-clean:
	rm -rf l10n/

.PHONY: l10n-read
l10n-read: ;

.PHONY: l10n-write
l10n-write: ;
