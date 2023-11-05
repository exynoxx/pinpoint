#include "TextChangedHandler.h"

void TextChangedHandler::handle(const QString& input) {

    auto result = QVariantList();
    auto textResults = csharp.SendText(input.toStdString());

    // Convert the textResults to QVariantList
    for (const string& textResult : textResults) {
        result.append(QString::fromStdString(textResult));
    }

    // Emit the result signal
    emit updatedResultSignal(result);
}
