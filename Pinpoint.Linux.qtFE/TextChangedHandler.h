#ifndef TEXTCHANGEDHANDLER_H
#define TEXTCHANGEDHANDLER_H

#endif // TEXTCHANGEDHANDLER_H
#include <CSharpCommunicator.hpp>
#include <QObject>
#include <QVariant>

class TextChangedHandler : public QObject {
    Q_OBJECT


public:
    TextChangedHandler(CSharpCommunicator& csharp, QObject* parent = nullptr) : QObject(parent), csharp(csharp) {}

private:
    CSharpCommunicator &csharp;


public slots:
    void handle(const QString& input);

signals:
    void updatedResultSignal(QVariantList result);
};
