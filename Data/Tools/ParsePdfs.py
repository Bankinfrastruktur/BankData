#!/usr/bin/env python
import camelot
import PyPDF2
import regex as re

def fixfield(v):
    v = str(v).replace("\n", "").replace("  ", " ").strip()
    if "Dataclearingen" in v:
        v = re.sub(r"(?i)" + re.escape(" (deltar endast i Dataclearingen, ej i Bankgirosystemet)"), "", v).strip()
    if " (fd. " in v:
        v = re.sub(r" \(fd\. [^)]+\)", "", v).strip()
    if " (f.d. " in v:
        v = re.sub(r" \(f\.d\. [^)]+\)", "", v).strip()
    return v

def splittomultiplerows(row):
    clrfld = re.sub(r' *\p{Dash} *', '-', row[1]).strip()
    clrflds = [c.strip() for c in clrfld.split('\n')]
    # combine continuation ranges to single range
    if len(clrflds) == 2:
        e1 = clrflds[0][-4:]
        s2 = clrflds[1][0:4]
        if int(e1)+1 == int(s2):
            clrflds = [f"{clrflds[0][0:4]}-{clrflds[1][-4:]}"]
    # split rows
    for clr in clrflds:
        rc = [r for r in row]
        rc[1] = clr
        yield rc

def strrow(row):
    return " ".join(fixfield(v) for v in row).strip()

def parsebankgirot(file, url, archivedate):
    # extract document date, often delay between document and release
    pdfdate = ""
    with open(file, "rb") as pdf_file:
        pdfread = PyPDF2.PdfFileReader(pdf_file)
        pagetext = pdfread.pages[0].extractText()
        m = re.search(r"(20[0-9]{2}-[0-1][0-9]-[0-3][0-9])", pagetext)
        pdfdate = m.group(0)
    yield f"# {url} {pdfdate}"

    tables = camelot.read_pdf(file, pages="all")
    nextheaderistype2 = False
    for t in tables:
        if t.accuracy < 99 or len(t.cols) != 4:
            nextheaderistype2 = True
            continue
        if nextheaderistype2:
            yield "# Typ 2"
        for row in t.df.itertuples(index=False):
            if "Bankens namn" in row[0]:
                # ignore headers
                continue
            for r in splittomultiplerows(row):
                yield strrow(r)

def parseibanid(file, url, archivedate):
    yield f"# {url}"
    tables = camelot.read_pdf(file, pages="all")
    for t in tables:
        if t.accuracy < 99 or len(t.cols) != 5:
            continue
        for row in t.df.itertuples(index=False):
            if "Clearing number" in row[0]:
                # ignore header
                continue
            # column 2 of iban file should be ibanid
            pfx = "# " if not row[1].isdigit() else ""
            yield pfx + strrow(row)

def getparser(file):
    if "bankernaskontonummeruppbyggnad" in file:
        return [parsebankgirot, "Bankgirot.txt"]
    if "iban-id" in file:
        return [parseibanid, "IbanBic.txt"]
    return None

if __name__ == "__main__":
    with open('.pdfstats', 'r') as fpdfstats:
        while line := fpdfstats.readline():
            spl = line.rstrip().split('|')
            file = spl[0]
            parser = getparser(file)
            if parser == None:
                print(f"Not used file {file}")
                continue
            data = parser[0](spl[0], spl[1], spl[2])
            with open(f"Data/{parser[1]}", 'w') as wf:
                wf.writelines(l + '\n' for l in data)
